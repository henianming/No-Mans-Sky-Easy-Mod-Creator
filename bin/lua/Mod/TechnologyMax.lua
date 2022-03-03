local mod = {
    name = "科技属性固定最优",
    modify = {
        {
            file = [[METADATA\REALITY\TABLES\NMS_REALITY_GCPROCEDURALTECHNOLOGYTABLE.MBIN]],
            modify = {
                --属性个数应该是越多越好吧，不判断了
                {
                    selector = "Table/!/NumStatsMin",
                    valueFunction = function(curNode)
                        local max = curNode:N("../NumStatsMax")[0]:V()
                        return max
                    end
                },
                {
                    selector = "Table/!/NumStatsMax",
                    valueFunction = function(curNode)
                        local max = curNode:N("../NumStatsMax")[0]:V()
                        return max
                    end
                },
                {
                    selector = "Table/!/WeightingCurve/WeightingCurve",
                    value = "NoWeighting"
                },
                --具体属性值需要根据具体情况取最大值还是最小值
                {
                    selector = "Table/!/StatLevels/!/ValueMin",
                    valueFunction = function(curNode)
                        local attributeName = curNode:N("../Stat/StatsType")[0]:V()
                        local min = curNode:N("../ValueMin")[0]:V()
                        local max = curNode:N("../ValueMax")[0]:V()
                        local mode = curNode:N("../WeightingCurve/WeightingCurve")[0]:V()

                        local needReverse = false

                        --这几个属性需要反着来，比如护盾强度属性是好的反而更常见
                        if attributeName == "Suit_Armour_Shield_Strength" then
                            needReverse = true
                        end

                        if mode == "NoWeighting" or mode == "MinIsUncommon" or mode == "MinIsRare" or mode == "MinIsSuperRare" then
                            return needReverse == false and min or max
                        elseif mode == "MaxIsUncommon" or mode == "MaxIsRare" or mode == "MaxIsSuperRare" then
                            return needReverse == false and max or min
                        else
                            Program.Log(mode, ConsoleColor.Green)
                            return ""
                        end
                    end
                },
                {
                    selector = "Table/!/StatLevels/!/ValueMax",
                    valueFunction = function(curNode)
                        local attributeName = curNode:N("../Stat/StatsType")[0]:V()
                        local min = curNode:N("../ValueMin")[0]:V()
                        local max = curNode:N("../ValueMax")[0]:V()
                        local mode = curNode:N("../WeightingCurve/WeightingCurve")[0]:V()

                        local needReverse = false

                        --这几个属性需要反着来，比如护盾强度属性是好的反而更常见
                        if attributeName == "Suit_Armour_Shield_Strength" then
                            needReverse = true
                        end

                        if mode == "NoWeighting" or mode == "MinIsUncommon" or mode == "MinIsRare" or mode == "MinIsSuperRare" then
                            return needReverse == false and min or max
                        elseif mode == "MaxIsUncommon" or mode == "MaxIsRare" or mode == "MaxIsSuperRare" then
                            return needReverse == false and max or min
                        else
                            Program.Log(mode, ConsoleColor.Green)
                            return ""
                        end
                    end
                },
                {
                    selector = "Table/!/StatLevels/!/WeightingCurve/WeightingCurve",
                    value = "NoWeighting"
                }
            }
        }
    }
}

return mod
