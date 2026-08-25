# OCR 工具

一个使用 PP-OCRv6 small 的本地 OCR 项目，包含 Windows 截图托盘应用和可部署到 Linux 的 PDF OCR API。

OCR 推理全部在本机完成，不调用云端 OCR 接口。当前使用 ONNX Runtime CPU 执行提供程序，不要求独立显卡或显存。Windows 桌面端与 PDF API 共用同一套 OCR 引擎和模型。

## 当前状态

当前桌面版本为 1.0.0，默认截图快捷键为 F2。

已完成的发布验证：

- 核心单元测试：7 项
- Windows 集成测试：19 项
- API 测试：10 项
- 合计：36 项，全部通过
- Windows 绿色版已从最终发布目录实际启动
- MSI 已完成构建、文件校验和元数据检查，构建结果为 0 警告、0 错误

为了不向系统盘写入测试文件，本机没有执行 MSI 的实际安装与卸载流程。

## 使用

1. 运行 `artifacts\publish\win-x64\OcrTool.App.exe`。
2. 按 `F2`。
3. 拖动鼠标框选文字区域。
4. 松开鼠标后会打开结果窗口：左侧显示原图，右侧文字可直接编辑。
5. 识别文字会按照设置自动复制，也可以编辑后点击“复制文字”。

按 `Esc` 或鼠标右键可取消框选。双击托盘图标也可开始识别，右键托盘图标可退出。

托盘菜单中的“设置…”可以：

- 直接录制新的全局快捷键
- 控制是否显示结果窗口
- 控制是否自动复制识别文字
- 自由选择是否在登录 Windows 后自动启动

## Windows 发布包

生成版本号为 `1.0.0` 的绿色版 ZIP 和安装版 MSI：

```powershell
& '.\eng\release-windows.ps1' -Version '1.0.0'
```

输出到 `artifacts\release\1.0.0`：

- `OcrTool-1.0.0-win-x64-portable.zip`：绿色版，解压后运行 `OcrTool\OcrTool.App.exe`；设置保存在程序目录。
- `OcrTool-1.0.0-win-x64-setup.msi`：安装版，需要管理员权限，安装到 Program Files，并创建桌面和开始菜单快捷方式；设置保存在当前用户的 LocalAppData。

两种发布包都是 Windows x64 自包含版本，用户无需另装 .NET、ONNX Runtime 或 OCR 模型。当前版本未进行代码签名，首次运行时 Windows 可能显示安全提醒。

### 两种发布形式的区别

| 项目 | 绿色版 | 安装版 |
| --- | --- | --- |
| 启动方式 | 解压后运行 OcrTool.App.exe | 双击 MSI 安装后从快捷方式启动 |
| 程序位置 | 用户选择的解压目录 | 64 位 Program Files |
| 设置位置 | 程序目录/settings.json | %LOCALAPPDATA%/OcrTool/settings.json |
| 快捷方式 | 不自动创建 | 创建桌面和开始菜单快捷方式 |
| 卸载 | 关闭程序后移走整个目录 | Windows“已安装的应用” |
| 管理员权限 | 不需要 | 安装时需要 |

绿色版依靠程序目录中的 portable.flag 判断存储模式。安装包会排除该标记，因此安装到 Program Files 后不会尝试在程序目录写入设置。

## PDF OCR API

API 接收 `multipart/form-data` PDF 文件，将 PDF 渲染为 180 DPI 图片并逐页识别，返回保持原页序的 JSON。

本机启动：

```powershell
& '.\eng\dotnet.ps1' run --project '.\src\OcrTool.Api\OcrTool.Api.csproj' --urls 'http://127.0.0.1:5080'
```

接口：

- `GET /health`：服务健康检查
- `POST /api/ocr/pdf`：表单字段名为 `file`，文件类型为 PDF

调用示例：

```powershell
curl.exe --request 'POST' `
    --form 'file=@F:\Docs\sample.pdf;type=application/pdf' `
    'http://127.0.0.1:5080/api/ocr/pdf'
```

返回结构：

```json
{
  "pageCount": 2,
  "pages": [
    { "pageNumber": 1, "text": "第一页文字" },
    { "pageNumber": 2, "text": "第二页文字" }
  ]
}
```

PDF API 的处理方式：

1. 接收 multipart/form-data 中名为 file 的 PDF 文件。
2. PDFium 以 180 DPI 异步逐页渲染。
3. 每页生成一张 SKBitmap 后立即交给共享 OCR 引擎。
4. 依次执行文本检测、方向分类和文字识别。
5. 保存页码与文字，并在该页处理结束后释放位图。
6. 返回 PageCount 和保持原页序的 Pages 数组。

页面按需产生，不需要先把整份 PDF 的所有页面同时放入内存。当前 API 没有身份认证；部署到非受信网络时，应由反向代理或网关负责访问控制、HTTPS 和上传大小限制。

## Ubuntu 部署

生成不需要预装 .NET 的 Linux x64 自包含版本：

```powershell
& '.\eng\publish-api-linux.ps1'
```

输出目录为 `artifacts\publish\api-linux-x64`，完整目录约 182 MiB。将整个目录复制到 Ubuntu，然后运行：

```bash
cd /opt/ocr-tool
ASPNETCORE_URLS='http://0.0.0.0:5080' ./OcrTool.Api
```

在本机 WSL 中可直接运行 F 盘发布目录：

```powershell
wsl.exe -d 'Ubuntu-22.04' `
    --cd '/mnt/f/Code/Ocr-tool/artifacts/publish/api-linux-x64' `
    --exec env 'ASPNETCORE_URLS=http://0.0.0.0:5080' './OcrTool.Api'
```

该发布已包含 .NET 运行时、PDFium、ONNX Runtime、SkiaSharp 和 OCR 模型，不需要在 Ubuntu 额外安装这些组件。桌面截图应用仍然只运行在 Windows，Linux 部署的是无界面的 API 服务。

## 技术方案

- .NET 10 WPF 托盘应用与 ASP.NET Core API
- ONNX Runtime CPU 执行提供程序，不使用显卡或显存
- PDFtoImage 5.4.0 / PDFium 逐页渲染
- RapidOcrNet 4.0.2
- PP-OCRv6 small 多语言检测与识别模型
- PP-OCRv6 专用预处理：短边自适应到 736、无 v5 白边填充
- 一个 PP-OCRv5 mobile 方向分类器，用于纠正 180° 文本方向

模型来自 [RapidOCR 官方模型清单](https://github.com/RapidAI/RapidOCR/blob/main/python/rapidocr/default_models.yaml)，模型使用方式对应 [RapidOcrNet](https://github.com/BobLd/RapidOcrNet) 的 `PPOCRv6Small` 预设。

### 技术路线

| 层次 | 技术 | 作用 |
| --- | --- | --- |
| 桌面 UI | WPF + Windows Forms | 设置窗口、结果窗口、托盘和全屏框选层 |
| 运行平台 | .NET 10 | 桌面应用、共享类库和 ASP.NET Core API |
| OCR 封装 | RapidOcrNet 4.0.2 | 加载 PaddleOCR ONNX 模型并组织推理 |
| 推理运行时 | ONNX Runtime CPU | 本地 CPU 推理 |
| 图像处理 | SkiaSharp | OCR 输入与 PDF 页面位图 |
| PDF 渲染 | PDFtoImage 5.4.0 / PDFium | PDF 页面转为 180 DPI 位图 |
| HTTP 服务 | ASP.NET Core Minimal API | 健康检查和 PDF OCR 接口 |
| Windows 安装 | WiX Toolset 6 | 生成 x64 MSI 和系统快捷方式 |
| 自动化测试 | xUnit + ASP.NET Core TestServer | 核心逻辑、真实模型、桌面辅助逻辑和 API |

### 模型组成

| 阶段 | 模型或资源 | 文件 |
| --- | --- | --- |
| 文本检测 | PP-OCRv6 small detection | models/v6/PP-OCRv6_det_small.onnx |
| 方向分类 | PP-LCNet x0.25 textline orientation mobile | models/v5/ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx |
| 文字识别 | PP-OCRv6 small recognition | models/v6/PP-OCRv6_rec_small.onnx |
| 字符映射 | PP-OCRv6 字典 | models/v6/ppocrv6_dict.txt |

### 模块架构

~~~mermaid
flowchart LR
    User[Windows 用户] --> App[OcrTool.App<br/>WPF 托盘应用]
    Client[HTTP 客户端] --> Api[OcrTool.Api<br/>PDF OCR API]

    App --> Core[OcrTool.Core<br/>设置、快捷键、选区几何]
    App --> Engine[OcrTool.Engine<br/>共享 OCR 引擎]
    Api --> Pdf[PDFtoImage / PDFium<br/>逐页渲染]
    Pdf --> Engine

    Engine --> Det[PP-OCRv6<br/>文本检测]
    Engine --> Cls[PP-LCNet<br/>方向分类]
    Engine --> Rec[PP-OCRv6<br/>文字识别]

    Installer[OcrTool.Installer<br/>WiX MSI] --> App
~~~

依赖方向：

- OcrTool.Core 不依赖 UI 和 OCR 运行时，存放可独立测试的设置、快捷键和选区逻辑。
- OcrTool.Engine 封装模型路径、RapidOcrNet 和 OCR 推理，供桌面端与 API 共用。
- OcrTool.App 依赖 Core 与 Engine，仅运行在 Windows。
- OcrTool.Api 依赖 Engine，可以在 Windows 或 Linux x64 运行。
- OcrTool.Installer 只负责把 Windows 自包含发布目录制作成 MSI。

### Windows 截图识别流程

~~~mermaid
sequenceDiagram
    participant U as 用户
    participant H as 全局快捷键/托盘
    participant S as 全屏框选层
    participant E as OCR 引擎
    participant R as 结果窗口
    participant C as 剪贴板

    U->>H: 按 F2 或点击托盘菜单
    H->>S: 创建截图框选层
    S->>S: 截取虚拟桌面并绘制遮罩
    U->>S: 拖动选择文字区域
    S-->>H: 返回裁剪后的位图
    opt 设置为显示结果窗口
        H->>R: 显示原图和识别中状态
    end
    H->>E: 提交位图
    E->>E: 检测 → 方向分类 → 识别
    E-->>H: 返回文字
    opt 设置为自动复制
        H->>C: 写入剪贴板
    end
    opt 已打开结果窗口
        H->>R: 填充可编辑文字
    end
~~~

桌面端的具体处理：

1. 使用 Windows RegisterHotKey 注册全局快捷键。
2. 截图层复制 SystemInformation.VirtualScreen，支持多显示器虚拟桌面。
3. 在内存截图上绘制遮罩、选区边框、尺寸提示和高对比度十字光标。
4. 松开鼠标后只克隆选择区域，并关闭全屏截图层。
5. OCR 模型在应用启动时通过后台任务初始化。
6. 根据设置把文字写入剪贴板、结果窗口，或同时输出到两者。
7. 一次 OCR 尚未结束时不会重复进入截图流程。

### PDF API 处理流程

~~~mermaid
flowchart TD
    Upload[POST /api/ocr/pdf<br/>multipart/form-data] --> Stream[打开 PDF 输入流]
    Stream --> Render[PDFium 以 180 DPI<br/>异步逐页渲染]
    Render --> Bitmap[SKBitmap 页面图像]
    Bitmap --> OCR[共享 OCR 引擎]
    OCR --> Page[生成页码与文本]
    Page --> More{还有页面?}
    More -- 是 --> Render
    More -- 否 --> Json[返回 PageCount + Pages JSON]
~~~

## 本地开发

仓库内的脚本会把 .NET、NuGet、临时目录和构建输出全部固定到当前 F 盘项目目录，不使用 C 盘或 D 盘缓存。

```powershell
& '.\eng\dotnet.ps1' restore '.\OcrTool.slnx'
& '.\eng\dotnet.ps1' test '.\OcrTool.slnx'
& '.\eng\run.ps1'
& '.\eng\publish.ps1'
```

主要目录：

- `.dotnet`：本项目专用 .NET 10 SDK
- `.nuget`：本项目专用 NuGet 包与缓存
- `.tmp`：本项目专用临时目录
- `assets\models`：PP-OCRv6 small 和方向分类器源资源
- `artifacts`：编译、测试和发布输出

### 构建输出与测试

项目固定使用 .NET SDK 10.0.400。eng/dotnet.ps1 会把 DOTNET_ROOT、NuGet 缓存、临时目录和构建输出指向当前项目目录。

完整测试：

~~~powershell
& '.\eng\dotnet.ps1' test '.\OcrTool.slnx' --no-restore
~~~

Windows 发布包校验：

~~~powershell
& '.\eng\verify-windows-release.ps1' -Version '1.0.0'
~~~

release-windows.ps1 的处理步骤：

1. 生成 Windows x64 自包含 Release 目录。
2. 写入版本号并排除 PDB。
3. 复制用户说明、第三方许可和绿色版标记。
4. 压缩绿色版 ZIP。
5. 使用 WiX 生成 MSI，并从安装包排除绿色版标记。
6. 校验 .NET 运行时、模型、字典、说明文件、ZIP 结构、MSI 和程序版本。

## 项目结构

| 路径 | 职责 |
| --- | --- |
| assets/models | OCR 检测、方向分类、识别模型和字典 |
| docs | 用户说明、第三方许可与绿色版标记 |
| eng | 本地 SDK 包装、运行、发布和发布校验脚本 |
| src/OcrTool.Core | 设置、快捷键和选区领域逻辑 |
| src/OcrTool.Engine | Windows/Linux 共用 OCR 引擎 |
| src/OcrTool.App | Windows WPF 生命周期、托盘、全局快捷键和截图 |
| src/OcrTool.Api | PDF 渲染、逐页 OCR 和 HTTP 接口 |
| src/OcrTool.Installer | WiX Windows x64 MSI 工程 |
| tests/OcrTool.Core.Tests | 核心单元测试 |
| tests/OcrTool.IntegrationTests | 真实 OCR 与 Windows 集成测试 |
| tests/OcrTool.Api.Tests | 跨平台引擎、PDF 服务和端点测试 |

## 配置与系统集成

- 默认设置：显示结果窗口、自动复制、不开机启动、快捷键 F2。
- 绿色版设置：程序目录/settings.json。
- 安装版设置：%LOCALAPPDATA%/OcrTool/settings.json。
- 开机启动：当前用户注册表 HKCU\Software\Microsoft\Windows\CurrentVersion\Run。
- 注册表值名：OcrTool；关闭设置中的启动选项时会移除该值。
- 当前没有开发 macOS 截图客户端。

## 第三方许可

发布目录中的 THIRD-PARTY-NOTICES.txt 列出了 RapidOcrNet、ONNX Runtime、SkiaSharp、Clipper2、PaddleOCR 模型和 .NET Runtime 等组件及许可链接。

## 代码目录索引

- `src\OcrTool.Core`：与 UI 无关的框选几何逻辑
- `src\OcrTool.Engine`：Windows/Linux 共用的 PP-OCRv6 small 推理引擎
- `src\OcrTool.App`：WPF 生命周期、全局快捷键、托盘和屏幕框选
- `src\OcrTool.Api`：PDF 渲染、逐页 OCR 和 HTTP 接口
- `tests\OcrTool.Core.Tests`：快速单元测试
- `tests\OcrTool.IntegrationTests`：真实 PP-OCRv6 small 推理测试
- `tests\OcrTool.Api.Tests`：跨平台引擎、PDF 逐页服务和 HTTP 端点测试
