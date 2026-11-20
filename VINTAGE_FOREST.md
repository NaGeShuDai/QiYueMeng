# 复古实木与窗口森林

## 文件

- `Poetry_Escape_Room.blend`：当前修改版。
- `Poetry_Escape_Room_before_vintage.blend`：修改前备份，包含本次连接时用户的场景调整。

## 材质与造型

墙壁改为深色橡木，新增木墙板、压条和顶梁；家具使用旧胡桃木，地板为旧橡木，窗帘换成木百叶。取消地毯。床品保留自然亚麻与烟草棕色；镜子、纸张、黄铜仍保留其实际材质。

## Blender 窗口森林

- 森林为真实网格，76 棵树，加地面和岩石。保存在 `11 Forest - remote portal world`，位于房间东侧约 80 米处。
- 窗口使用 Cycles Ray Portal；传送光线的位置加上 (80,0,0)，保持射线方向，形成真实视差。
- 森林材质根据 Portal Depth 显示：未经窗口传送的 Cycles 射线看到透明对象。窗口背面显示木色。
- `01 Portal Preview - Cycles`：用于 Cycles 渲染。Solid / Material Preview 不展示完整传送效果。
- `02 Modeling - room only`：排除森林集合，适合普通建模。该层默认不参与渲染。
- 仅房间建模层无法同时渲染森林；需要预览窗口时切回 Portal Preview 层并用 Cycles。
- `CAM 04` 为新的室内预览；`CAM 05 / 06` 用于检查不同观察位置的视差。
- 默认启用 `09 Cutaway Shell`，完整封闭房间；隐藏该集合并使用 CAM 01 可恢复剖视总览。

## 导入 Unity

Blender 的 Ray Portal、Portal Depth 和程序化木纹节点不会自动变成 Unity 效果。本项目目录尚无 Unity 工程，因此本轮未配置或验证 Unity 实现。

推荐保留森林为独立模型与 Forest 图层；主相机剔除 Forest，森林相机只渲染 Forest 至 RenderTexture。窗口平面显示该纹理。森林相机按玩家相对窗口的位置同步，使用窗口四角确定离轴投影或等价的正确屏幕投影，避免把普通摄像头画面贴在窗户上造成错误视差。窗口外侧应封闭或隐藏显示面，必要时用 stencil 限制窗口区域。

Scene 视图的森林可见性单独关闭，仅作编辑隐藏，不禁用森林 GameObject，以免影响游戏相机。不同 Unity 渲染管线需要各自的材质/遮罩实现；待确定 URP/HDRP/Built-in 后接入。

导出时木纹应烘焙为 PBR 贴图或重建 Unity 材质；森林材质应去掉 Blender Portal Depth 条件，改用 Unity 图层隔离。
