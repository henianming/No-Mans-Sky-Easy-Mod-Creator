local mod = {
    name = "掉落修改",
    modify = {
        {
            file = [[METADATA\REALITY\TABLES\REWARDTABLE.MBIN]],
            modify = {
                {
                    selector = "aa",
                    value = 2
                }
            }
        }
    }
}

return mod
