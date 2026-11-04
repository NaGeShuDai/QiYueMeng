# 诗集密室 · 浏览与聚焦原型

打开 `Scenes/PoetryBedroom.unity` 后进入 Play。

- 按住鼠标左键或右键拖动：无惯性直接环绕房间，水平和垂直旋转均不限角度，可翻越顶部和底部。
- A / D 或左右方向键：水平旋转。
- 滚轮：小幅拉近/拉远总览。
- 单击书桌、书架、床头柜、衣柜镜子、床、窗户、门、墙上诗页：平滑聚焦。
- Esc：返回进入聚焦前的总览角度和距离。
- 聚焦衣柜或抽屉后，点击门板、抽屉面板或把手可打开/关闭；开关状态在本次 Play 中保留。

天花板始终不显示。墙体、对应墙框和挂件一并按观察方向显隐；最多显示两组墙。聚焦时仅显示当前目标和地板，隐藏其他家具与墙面；返回恢复。柜门、抽屉可开关，没有道具拾取、解锁或谜题状态。

## 森林窗口

房间和森林来自独立 FBX，木纹使用随附的 4K PNG，不依赖 Blender。

森林使用 Layer 30，只有森林相机能绘制；主相机排除此层。森林相机与玩家相机具有相同方向、投影和相对位置，但位置平移 80 米至森林区域。窗口表面按屏幕投影读取森林相机的 RenderTexture，保持视差和遮挡。窗口所在墙被隐藏时森林相机停止渲染。Scene 视图已单独隐藏森林对象，Play 不受影响。

Layer 29 用于聚焦点击区域；Layer 28 用于柜门/抽屉点击网格；Layer 30 用于森林。请勿将这三个图层复用于其他系统。

当前镜面是静态金属材质，未实现实时镜面反射。

## 编辑

`Room View and Focus` 上配置墙体组、聚焦点、相机速度与距离。每个 `Focus - ...` 对象保存目标点、相机位置和点击区域。

`Tools / Poetry Room / Build Scene` 会从 FBX 重建初版场景和材质，并丢失后续镜头、门和抽屉修改；普通使用只需打开已保存场景并 Play。若有意重建，需要在初版生成后执行 `Apply View Revision` 和 `Finish Revision`。`Apply View Revision` 禁止对已修改场景重复执行。

`Tools / Poetry Room / Validate Play` 在 Play 模式执行鼠标/键盘输入与视角检查；报告和截图保存到工程外层 `UnityValidation`。

## Unity MCP

插件在 `LocalPackages/com.coplaydev.unity-mcp`，通过编辑器内 TCP bridge 连接。`Assets/Editor/PuzzleMcpConnect.cs` 在载入时启用此连接；不依赖后台 Python MCP 服务。外层 `unity_client.py` 是本次操作使用的客户端。
