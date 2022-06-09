# ResourceEditer
Azure Resources Tag Parser

# 단일 실행 파일 만들기
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true
```