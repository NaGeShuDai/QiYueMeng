from pathlib import Path
import hashlib, json, sys
root = Path(__file__).resolve().parent
manifest = json.loads((root / "BACKUP_MANIFEST.json").read_text(encoding="utf-8"))
errors = []
for item in manifest["files"]:
    path = root / item["path"]
    if not path.is_file() or path.stat().st_size != item["size"] or hashlib.sha256(path.read_bytes()).hexdigest() != item["sha256"]:
        errors.append(item["path"])
print(f"Verified {len(manifest['files'])} files; failures: {len(errors)}")
for error in errors: print(error)
sys.exit(bool(errors))
