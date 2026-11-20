"""Loopback-only memorial site. Persistent data lives beside this file."""
import argparse
import datetime
import json
import os
import threading
import webbrowser
import re
import base64
import tempfile
import zipfile
import shutil
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
from urllib.request import urlopen

ROOT = Path(__file__).resolve().parent
LOCK = threading.Lock()
EMPTY = {"version": 1, "albums": [], "dates": []}
PHOTO_REF = re.compile(r'data/photos/[A-Za-z0-9_-]+\.(?:jpg|jpeg|png|webp)')

def photo_refs(data):
    return {p for a in data['albums'] for p in a['photos'] if PHOTO_REF.fullmatch(p)}

def validate(data):
    if not isinstance(data, dict) or data.get('version') != 1:
        raise ValueError('Invalid collection')
    ids = set()
    for kind in ('albums', 'dates'):
        if not isinstance(data.get(kind), list):
            raise ValueError('Invalid collection')
        for item in data[kind]:
            if not isinstance(item, dict):
                raise ValueError('Invalid record')
            for key, limit in [('id', 200), ('title', 100), ('text', 20000), ('date', 10)]:
                if not isinstance(item.get(key), str) or len(item[key]) > limit:
                    raise ValueError('Invalid ' + key)
            if not item['title'].strip() or not item['id'] or item['id'] in ids:
                raise ValueError('Invalid title or duplicate id')
            ids.add(item['id'])
            if item['date']:
                datetime.date.fromisoformat(item['date'])
            elif kind == 'dates':
                raise ValueError('Date required')
            for key in ('dateEnd', 'dateLabel'):
                if key in item and (not isinstance(item[key], str) or len(item[key]) > 100):
                    raise ValueError('Invalid date range')
            if item.get('dateEnd'):
                datetime.date.fromisoformat(item['dateEnd'])
                if not item['date'] or item['dateEnd'] < item['date']:
                    raise ValueError('End date precedes start date')
            if kind == 'albums':
                if not isinstance(item.get('photos'), list):
                    raise ValueError('Invalid photos')
                for photo in item['photos']:
                    if isinstance(photo, str) and PHOTO_REF.fullmatch(photo):
                        continue
                    if not isinstance(photo, str) or not re.fullmatch(r'data:image/(jpeg|png|webp);base64,[A-Za-z0-9+/]+=*', photo):
                        raise ValueError('Invalid photo')
                    base64.b64decode(photo.split(',', 1)[1], validate=True)
            elif not isinstance(item.get('annual'), bool):
                raise ValueError('Invalid anniversary')

class Handler(SimpleHTTPRequestHandler):
    def end_headers(self):
        if self.path.split('?', 1)[0].endswith(('.html', '.js', '.css')) or self.path == '/':
            self.send_header('Cache-Control', 'no-store')
        super().end_headers()

    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(ROOT), **kwargs)

    def log_message(self, *args):
        pass

    def allowed(self):
        expected = '127.0.0.1:' + str(self.server.server_port)
        return self.headers.get('Host') == expected and self.headers.get('Origin', 'http://' + expected) == 'http://' + expected

    def reply(self, code, value):
        body = json.dumps(value, ensure_ascii=False).encode('utf-8')
        self.send_response(code)
        self.send_header('Content-Type', 'application/json; charset=utf-8')
        self.send_header('Cache-Control', 'no-store')
        self.send_header('Content-Length', str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def save(self, data, revision):
        validate(data)
        for ref in photo_refs(data):
            if not (self.server.data_file.parent / ref.removeprefix('data/')).is_file():
                raise ValueError('Photo missing; restore the full data folder: ' + ref)
        current = json.loads(self.server.data_file.read_text('utf-8'))
        if revision != current.get('revision', 0):
            return False
        data['revision'] = current.get('revision', 0) + 1
        temp = self.server.data_file.with_suffix('.tmp')
        with temp.open('w', encoding='utf-8') as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
            f.flush()
            os.fsync(f.fileno())
        self.server.data_file.with_suffix('.previous.json').write_bytes(self.server.data_file.read_bytes())
        os.replace(temp, self.server.data_file)
        return True

    def export_backup(self):
        # Disk-backed archive avoids putting gigabytes of original photos in RAM.
        with tempfile.TemporaryFile() as output:
            with LOCK:
                data = json.loads(self.server.data_file.read_text('utf-8'))
                validate(data)
                with zipfile.ZipFile(output, 'w', compression=zipfile.ZIP_STORED, allowZip64=True) as archive:
                    archive.writestr('collection.json', json.dumps(data, ensure_ascii=False))
                    for ref in sorted(photo_refs(data)):
                        local = ref.removeprefix('data/')
                        archive.write(self.server.data_file.parent / local, local)
                        preview = 'previews/' + Path(local).stem + '.jpg'
                        if (self.server.data_file.parent / preview).is_file():
                            archive.write(self.server.data_file.parent / preview, preview)
            length = output.tell()
            output.seek(0)
            self.send_response(200)
            self.send_header('Content-Type', 'application/zip')
            self.send_header('Content-Disposition', 'attachment; filename="SeventhPier-backup.zip"')
            self.send_header('Content-Length', str(length))
            self.send_header('Cache-Control', 'no-store')
            self.end_headers()
            shutil.copyfileobj(output, self.wfile, 1024 * 1024)

    def restore_backup(self):
        length = int(self.headers.get('Content-Length', 0))
        if not 0 < length <= 16 * 1024**3:
            raise ValueError('Backup must be smaller than 16 GB')
        with tempfile.TemporaryFile() as upload:
            remaining = length
            while remaining:
                chunk = self.rfile.read(min(1024 * 1024, remaining))
                if not chunk:
                    raise ValueError('Incomplete backup')
                upload.write(chunk)
                remaining -= len(chunk)
            upload.seek(0)
            with zipfile.ZipFile(upload) as archive, tempfile.TemporaryDirectory(dir=self.server.data_file.parent) as temp:
                members = archive.infolist()
                if len(members) > 50000 or sum(m.file_size for m in members) > 16 * 1024**3:
                    raise ValueError('Backup too large')
                names = {m.filename for m in members}
                if len(names) != len(members) or 'collection.json' not in names or archive.getinfo('collection.json').file_size > 256*1024**2:
                    raise ValueError('Invalid archive')
                data = json.loads(archive.read('collection.json'))
                validate(data)
                refs = {p.removeprefix('data/') for p in photo_refs(data)}
                if not refs.issubset(names):
                    raise ValueError('Backup is missing photos')
                for member in members:
                    name = member.filename
                    if name == 'collection.json':
                        continue
                    if not re.fullmatch(r'(photos/[A-Za-z0-9_-]+\.(jpg|jpeg|png|webp)|previews/[A-Za-z0-9_-]+\.jpg)', name):
                        raise ValueError('Invalid archive path')
                    target = Path(temp) / name
                    target.parent.mkdir(exist_ok=True)
                    with archive.open(member) as source, target.open('wb') as dest:
                        shutil.copyfileobj(source, dest, 1024*1024)
                with LOCK:
                    revision = int(self.headers.get('X-Story-Revision', '-1'))
                    current = json.loads(self.server.data_file.read_text('utf-8'))
                    if revision != current.get('revision', 0):
                        return self.reply(409, {'error': '内容已变化，请刷新后再导入。'})
                    import hashlib
                    for name in names - {'collection.json'}:
                        target = self.server.data_file.parent / name
                        source = Path(temp) / name
                        if target.exists():
                            with target.open('rb') as f, source.open('rb') as g:
                                if hashlib.file_digest(f, 'sha256').digest() != hashlib.file_digest(g, 'sha256').digest():
                                    raise ValueError('Different photo already exists: ' + name)
                    for name in names - {'collection.json'}:
                        target = self.server.data_file.parent / name
                        target.parent.mkdir(exist_ok=True)
                        if not target.exists():
                            os.replace(Path(temp) / name, target)
                    self.save(data, revision)
        return self.reply(200, data)

    def do_GET(self):
        if not self.allowed():
            return self.reply(403, {'error': 'Forbidden origin'})
        if self.path == '/api/health':
            return self.reply(200, {'app': 'SeventhPier', 'root': str(ROOT)})
        if self.path == '/api/backup':
            return self.export_backup()
        if self.path == '/api/collection':
            try:
                with LOCK:
                    data = json.loads(self.server.data_file.read_text('utf-8'))
                    validate(data)
                return self.reply(200, data)
            except Exception as exc:
                return self.reply(500, {'error': 'Cannot read data: ' + str(exc)})
        # Only serve the public website, never scripts, backups or directory listings.
        from urllib.parse import unquote, urlsplit
        path = (ROOT / unquote(urlsplit(self.path).path).lstrip('/')).resolve()
        route = unquote(urlsplit(self.path).path).lstrip('/')
        if re.fullmatch(r'data/(photos/[A-Za-z0-9_-]+\.(jpg|jpeg|png|webp)|previews/[A-Za-z0-9_-]+\.jpg)', route):
            image = self.server.data_file.parent / route.removeprefix('data/')
            if not image.is_file():
                return self.reply(404, {'error': 'Photo not found'})
            self.send_response(200)
            self.send_header('Content-Type', self.guess_type(str(image)))
            self.send_header('Content-Length', str(image.stat().st_size))
            self.send_header('Cache-Control', 'private, max-age=3600')
            self.end_headers()
            with image.open('rb') as f:
                shutil.copyfileobj(f, self.wfile)
            return
        if path == ROOT:
            self.path = '/index.html'
            path = ROOT / 'index.html'
        if ROOT not in path.parents or path.suffix.lower() not in {'.html', '.css', '.js', '.png', '.jpg', '.jpeg', '.webp', '.svg', '.ico', '.wav'} or 'data' in path.relative_to(ROOT).parts:
            return self.reply(404, {'error': 'Not found'})
        super().do_GET()

    def do_POST(self):
        if not self.allowed() or self.headers.get('X-Story-Request') != 'save':
            return self.reply(403, {'error': 'Forbidden request'})
        if self.path == '/api/restore':
            try:
                return self.restore_backup()
            except Exception as exc:
                return self.reply(400, {'error': str(exc)})
        if self.path != '/api/collection':
            return self.reply(404, {'error': 'Not found'})
        try:
            length = int(self.headers.get('Content-Length', 0))
            if not 0 < length <= 256 * 1024 * 1024:
                raise ValueError('Collection exceeds 256 MB')
            request = json.loads(self.rfile.read(length))
            data = request['data']
            with LOCK:
                if not self.save(data, request.get('revision')):
                    return self.reply(409, {'error': '另一窗口已修改内容，请刷新后重试（先复制当前文字）。'})
            self.reply(200, data)
        except Exception as exc:
            self.reply(400, {'error': str(exc)})

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=18777)
    parser.add_argument('--open', action='store_true')
    parser.add_argument('--data-dir', type=Path, default=ROOT / 'data')
    args = parser.parse_args()
    url = 'http://127.0.0.1:' + str(args.port)
    try:
        server = ThreadingHTTPServer(('127.0.0.1', args.port), Handler)
    except OSError:
        with urlopen(url + '/api/health', timeout=3) as response:
            health = json.load(response)
        if health != {'app': 'SeventhPier', 'root': str(ROOT)}:
            raise RuntimeError('Port belongs to another application')
        if args.open:
            webbrowser.open(url)
        return
    args.data_dir.mkdir(parents=True, exist_ok=True)
    server.data_file = args.data_dir / 'collection.json'
    if not server.data_file.exists():
        server.data_file.write_text(json.dumps(EMPTY), encoding='utf-8')
    if args.open:
        webbrowser.open(url)
    server.serve_forever()

if __name__ == '__main__':
    main()
