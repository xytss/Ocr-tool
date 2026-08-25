# OCR 工具

OCR 工具是一款运行在 Windows 10/11 x64 上的本地截图识别工具。按 F2 框选屏幕区域，识别结果会显示在窗口中，也可以自动复制到剪贴板。

截图和文字不会上传到云端。程序使用 CPU 运行 PP-OCRv6 small 模型，不需要独立显卡。

发布包还带有 PDF OCR API。API 默认不会启动，需要时手动运行即可；不用 API 时，不会占用端口或后台资源。

## 主要功能

- 按 F2 开始截图识别
- 支持多显示器
- 识别结果可以编辑和复制
- 可选择是否自动复制识别文字
- 可修改全局截图快捷键
- 可选择是否随 Windows 登录启动
- 提供 PDF OCR HTTP API
- 桌面端与 API 共用本地 OCR 模型

## 下载

从 [GitHub Releases](https://github.com/xytss/Ocr-tool/releases/latest) 下载最新版。

| 文件 | 适合的用途 |
| --- | --- |
| `OcrTool-x.y.z-win-x64-setup.msi` | Windows 安装版，安装后使用桌面或开始菜单快捷方式启动 |
| `OcrTool-x.y.z-win-x64-portable.zip` | Windows 绿色版，解压后直接运行，不写入 Program Files |
| `OcrTool-x.y.z-api-linux-x64.tar.gz` | Linux x64 PDF OCR API，不包含桌面截图程序 |

三个包都已包含 .NET 运行时、ONNX Runtime、OCR 模型和所需依赖，无需另外安装运行环境。

Windows 程序目前没有商业代码签名。安装或首次运行时，Windows 可能显示“未知发布者”或 SmartScreen 提示。

## Windows 安装

### MSI 安装版

1. 下载 `OcrTool-x.y.z-win-x64-setup.msi`。
2. 双击 MSI，按照 Windows Installer 提示完成安装。
3. 从桌面或开始菜单打开“OCR 工具”。

程序安装到 64 位 Program Files。卸载时，在 Windows 的“已安装的应用”中找到“OCR 工具”。

### 绿色版

1. 下载 `OcrTool-x.y.z-win-x64-portable.zip`。
2. 解压完整压缩包。
3. 运行解压目录中的 `OcrTool\OcrTool.App.exe`。

不要只复制 EXE。`models` 目录和其他 DLL 都是程序运行所需文件。绿色版不会自动创建快捷方式，也不需要管理员权限。

## 截图识别

1. 启动“OCR 工具”。
2. 按 F2。
3. 拖动鼠标框选需要识别的区域。
4. 松开鼠标，等待识别完成。
5. 在结果窗口中编辑或复制文字。

按 Esc 或鼠标右键可以取消框选。双击托盘图标也可以开始识别，右键托盘图标可以打开设置或退出程序。

如果 F2 已被其他程序占用，OCR 工具会打开设置窗口，请录入并保存新的快捷键。

## PDF OCR API

Windows 绿色版和 MSI 安装版都包含 API 程序。API 与桌面截图功能相互独立，不会随桌面程序自动启动。

### 在 Windows 上启动

绿色版：在解压后的 `OcrTool` 目录打开 PowerShell，运行：

~~~powershell
& '.\api\OcrTool.Api.exe' --urls 'http://127.0.0.1:5080'
~~~

MSI 安装版：

~~~powershell
& 'C:\Program Files\OCR Tool\api\OcrTool.Api.exe' --urls 'http://127.0.0.1:5080'
~~~

命令运行期间 API 保持启动。按 Ctrl+C 可以停止服务。

### 接口

| 方法和路径 | 作用 |
| --- | --- |
| `GET /health` | 检查服务是否正常 |
| `POST /api/ocr/pdf` | 上传 PDF 并按页返回识别文字 |

PDF 请求必须使用 `multipart/form-data`，文件字段名为 `file`。

PowerShell 调用示例：

~~~powershell
curl.exe --request 'POST' `
    --form 'file=@C:\Docs\sample.pdf;type=application/pdf' `
    'http://127.0.0.1:5080/api/ocr/pdf'
~~~

返回示例：

~~~json
{
  "pageCount": 2,
  "pages": [
    {
      "pageNumber": 1,
      "text": "第一页文字"
    },
    {
      "pageNumber": 2,
      "text": "第二页文字"
    }
  ]
}
~~~

PDF 会逐页处理，返回结果保持原页顺序。

### 在 Linux x64 上启动

下载 `OcrTool-x.y.z-api-linux-x64.tar.gz`，然后运行：

~~~bash
mkdir -p 'ocr-tool'
tar -xzf OcrTool-*-api-linux-x64.tar.gz -C 'ocr-tool'
cd 'ocr-tool'
ASPNETCORE_URLS='http://127.0.0.1:5080' ./OcrTool.Api
~~~

如果需要让其他设备访问，可以把监听地址改为 `http://0.0.0.0:5080`。

API 本身没有身份认证。不要把端口直接暴露到公网；对外提供服务时，请通过反向代理或网关配置 HTTPS、访问控制和上传大小限制。

发布包中附有 `API使用说明.txt`，包含相同的启动方式和调用示例。

## 设置和数据位置

| 项目 | 绿色版 | MSI 安装版 |
| --- | --- | --- |
| 设置文件 | 程序目录中的 `settings.json` | `%LOCALAPPDATA%\OcrTool\settings.json` |
| 快捷方式 | 不自动创建 | 桌面和开始菜单 |
| 管理员权限 | 不需要 | 安装时需要 |
| 卸载方式 | 关闭程序后移走解压目录 | Windows“已安装的应用” |

开机启动设置保存在当前用户账户中，可以随时在程序设置里关闭。

## 从源码运行

开发环境需要 PowerShell 7 和 .NET SDK 10.0.400。Windows 桌面端只能在 Windows 上运行。

~~~powershell
dotnet restore '.\OcrTool.slnx'
dotnet test '.\OcrTool.slnx'
dotnet run --project '.\src\OcrTool.App\OcrTool.App.csproj'
~~~

从源码启动 PDF OCR API：

~~~powershell
dotnet run --project '.\src\OcrTool.Api\OcrTool.Api.csproj' --urls 'http://127.0.0.1:5080'
~~~

主要项目：

- `src\OcrTool.App`：Windows 截图、托盘、设置和结果窗口
- `src\OcrTool.Api`：PDF OCR HTTP API
- `src\OcrTool.Engine`：桌面端和 API 共用的 OCR 引擎
- `src\OcrTool.Core`：设置、快捷键和选区逻辑
- `src\OcrTool.Installer`：Windows MSI 安装程序

## 第三方组件

程序使用 RapidOcrNet、ONNX Runtime、SkiaSharp、PDFium 和 PaddleOCR 模型。相关许可见 [THIRD-PARTY-NOTICES.txt](docs/THIRD-PARTY-NOTICES.txt)。
