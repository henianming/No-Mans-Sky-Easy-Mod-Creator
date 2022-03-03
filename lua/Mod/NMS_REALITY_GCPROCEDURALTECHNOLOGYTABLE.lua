local mod = {
    name = "科技全满",
    modify = {
        {
            file = [[METADATA\REALITY\TABLES\NMS_REALITY_GCPROCEDURALTECHNOLOGYTABLE.MBIN]],
            modify = {
                {
                    selector = "Table/!/StatLevels&ID==UP_LASER1/!/ValueMin",
                    valueFunction = function(curNode)
                        local vv = curNode:N("../ValueMax")[0]:V()
                        return vv
                    end
                }
            }
        }
    }
}

return mod
