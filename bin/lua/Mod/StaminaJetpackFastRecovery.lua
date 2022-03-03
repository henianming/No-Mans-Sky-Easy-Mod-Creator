local mod = {
    name = "玩家冲刺&火箭背包快速恢复",
    modify = {
        {
            file = [[GCPLAYERGLOBALS.GLOBAL.MBIN]],
            modify = {
                {
                    selector = "JetpackFillRate",
                    value = "5"
                },
                {
                    selector = "JetpackFillRateHardMode",
                    value = "2"
                },
                {
                    selector = "JetpackFillRateMidair",
                    value = "2.5"
                },
                {
                    selector = "StaminaRecoveryRate",
                    value = "1"
                }
            }
        }
    }
}

return mod
