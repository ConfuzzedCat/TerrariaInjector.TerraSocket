namespace TerraSocket
{
    public class CommandModel
    {
        public string UserName { get; set; }
        public string Command { get; set; }
        public int ItemID { get; set; }
        public int HealAmount { get; set; }
    }
}


/*
{
    "UserName": "userName",
    "Command":"KillPlayer",
    "ItemID" :1,
    "HealAmount" :10
}
*/