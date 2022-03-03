local config = {
    --原始EXML所在根目录
    rawRootPath = [[E:\MyProject\raw]],
    --中间临时文件存放路径
    tempRootPath = [[output]],
    --最后输出mod pak文件的路径
    outputRootPath = [[F:\公交补充\output.pak]]
}

json = require("json")

local IO = import("mscorlib", "System.IO")
local System = import("mscorlib", "System")
local WRSK = import("WRSK", "WRSK")

DirectoryInfo = IO.DirectoryInfo
ConsoleColor = System.ConsoleColor
File = IO.File
Program = WRSK.Program

function Log(content)
    Program.Log("--------------------------------------------------", ConsoleColor.Green)
    Program.Log(content, ConsoleColor.Green)
end

function DoString(code)
    local result = Program.DoString(code)
    local resultArr = {}
    for i = 1, result.Length, 1 do
        resultArr[i] = result[i - 1]
    end
    return table.unpack(resultArr)
end

local totalModify = {}

function AddModify(modify)
    local targetKey = nil
    for k, v in pairs(totalModify) do
        if v.file == modify.file then
            targetKey = k
            break
        end
    end

    if targetKey == nil then
        table.insert(
            totalModify,
            {
                file = modify.file,
                modify = {}
            }
        )
        targetKey = #totalModify
    end

    for k, v in pairs(modify.modify) do
        table.insert(totalModify[targetKey].modify, v)
    end
end

Log("读取mod修改代码")

local di = DirectoryInfo("./lua/Mod")
local fileArr = di:GetFiles()
for i = 1, fileArr.Length, 1 do
    local file = fileArr[i - 1]
    local streamReader = file:OpenText()
    local content = streamReader:ReadToEnd()
    local result = DoString(content)
    for k, v in pairs(result.modify) do
        AddModify(v)
    end
end

--print(json.encode(totalModify))

Log("获取原始游戏文件")

Program.InitDirectory(config.tempRootPath)

for k, v in pairs(totalModify) do
    Program.GetFile(config.rawRootPath, config.tempRootPath, v.file)
end

Log("MBINCompiler解码")

os.execute([[.\tool\MBINCompiler.exe convert -d .\]] .. config.tempRootPath .. [[\EXML .\]] .. config.tempRootPath .. [[\MBIN]])

Log("写入mod修改")
for k, v in pairs(totalModify) do
    for kk, vv in pairs(v.modify) do
        if vv.valueFunction == nil then
            Program.DoModify("./" .. config.tempRootPath .. "/EXML/" .. v.file, json.encode(vv))
        else
            local func = vv.valueFunction
            vv.valueFunction = nil
            Program.DoModifyF("./" .. config.tempRootPath .. "/EXML/" .. v.file, json.encode(vv), func)
        end
    end
end

Log("MBINCompiler编码")
os.execute([[.\tool\MBINCompiler.exe convert -d .\]] .. config.tempRootPath .. [[\MBIN_m .\]] .. config.tempRootPath .. [[\EXML]])

Log("压缩生成pak")

local fileListString = Program.GetPackFileListStr("./" .. config.tempRootPath .. "/MBIN_m")
os.execute([[.\tool\PSArcTool.exe ]] .. fileListString)

Program.Sleep(0.2)

File.Move("./" .. config.tempRootPath .. "/MBIN_m/psarc.pak", config.outputRootPath)
