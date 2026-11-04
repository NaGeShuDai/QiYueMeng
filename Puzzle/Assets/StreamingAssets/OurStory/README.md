# 第七码头 · 本地纪念网页

双击 index.html 即可在浏览器预览，无需联网或启动服务器。

编辑 content.js 可替换昵称、三段经历、信件与愿望。照片放入自建的 photos 文件夹，填写 photo，例如 `photos/us.jpg`。当前人物和经历为示例，页面已经注明。

游戏使用最终钥匙打开出口后展示漩涡，3.2 秒后调用外部默认浏览器。回到游戏可点击“打开纪念网页”再次访问。读取通关存档不会自动弹浏览器，重置进度会取消等待中的跳转。

网页位于 StreamingAssets，Windows 构建时随游戏打包。请保留完整的游戏 Data 文件夹。

Unity 的 Tools / Poetry Room / Validate Ending Portal 可在 Play 中检查结局；验证时拦截浏览器调用并在结束后恢复原谜题状态。
