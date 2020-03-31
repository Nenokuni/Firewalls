# 概要

# スタートガイド

Debugモードで実行する。
```
```

Releaseモードで実行する。
```
```

# 単一実行ファイルの生成

Windows10 64bit Debug
```
# Debug
dotnet publish -c Debug -r win10-x64 -p:PublishSingleFile=true

# Release
dotnet publish -c Release -r win10-x64 -p:PublishSingleFile=true
```

Linux 64bit Debug
```
# Debug
dotnet publish -c Debug -r linux-x64 -p:PublishSingleFile=true

# Release
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true
```