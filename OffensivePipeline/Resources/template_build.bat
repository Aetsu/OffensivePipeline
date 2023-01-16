call "{{MSBUILD_PATH}}"
msbuild.exe "{{SOLUTION_PATH}}" {{BUILD_OPTIONS}} /p:OutputPath="{{OUTPUT_DIR}}" /p:DebugSymbols=false /p:DebugType=None /p:AssemblyName="{{OUTPUT_FILENAME}}"