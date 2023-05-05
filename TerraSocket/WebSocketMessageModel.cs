namespace TerraSocket
{
    public class WebSocketMessageModel
    {
        public WebSocketMessageModel(string _event, bool status, ContextInfo contextInfo = null)
        {
            SetWebSocketServerVersion();
            Event = _event;
            Status = status ? "Success" : "Error";
            ExtraInfo = contextInfo;
        }

        public SemVersion webSocketServerVersion { get; private set; }
        public void SetWebSocketServerVersion()
        {
            webSocketServerVersion = new SemVersion();
        }
        public string Event { get; set; }
        public string Status { get; set; }
        public ContextInfo ExtraInfo { get; set; }

        public class SemVersion
        {
            public SemVersion()
            {
                MajorVersion = 1;
                MinorVersion = 0;
                PatchVersion = 5;
            }
            public int MajorVersion { get; set; }
            public int MinorVersion { get; set; }
            public int PatchVersion { get; set; }
            public string ToStringDot()
            {
                return string.Format("{0}.{1}.{2}",MajorVersion,MinorVersion,PatchVersion);
            }
        }

        public class ContextInfo
        {
            public ContextInfo(string player = null, params object[] Info)
            {
                Player = player;
                foreach (var item in Info)
                {
                    if (item is string) Eventinfo = item as string;
                    if (item is ContextPlayerDamage) PlayerDamage = item as ContextPlayerDamage;
                    if (item is ContextNPCDamage) NPCDamage = item as ContextNPCDamage;
                    if (item is ContextNPCKilled) NPCKilled = item as ContextNPCKilled;
                    if (item is ContextPlayerKilled) PlayerKilled = item as ContextPlayerKilled;
                    if (item is ContextBossSpawn) BossSpawn = item as ContextBossSpawn;
                }
            }
            public string Player { get; set; }
            public string Eventinfo { get; set; }
            public ContextPlayerDamage PlayerDamage { get; set; }
            public ContextNPCDamage NPCDamage { get; set; }
            public ContextNPCKilled NPCKilled { get; set; }
            public ContextPlayerKilled PlayerKilled { get; set; }
            public ContextBossSpawn BossSpawn { get; set; }
            public class ContextPlayerDamage
            {
                public ContextPlayerDamage(string playername, double damage = 0, bool crit = false, bool pvp = false, bool quiet = false, int hitDirection = 0, string sourceType = null, string sourceName = null)
                {
                    PlayerName = playername;
                    Damage = damage;
                    Crit = crit;
                    PVP = pvp;
                    Quiet = quiet;
                    HitDirection = hitDirection;
                    SourceType = sourceType;
                    SourceName = sourceName;
                }
                public string PlayerName { get; set; }
                public bool PVP { get; set; }
                public bool Quiet { get; set; }
                public double Damage { get; set; }
                public int HitDirection { get; set; }
                public bool Crit { get; set; }
                public string SourceType { get; set; }
                public string SourceName { get; set; }
            }

            public class ContextNPCDamage
            {
                public ContextNPCDamage(string npcName = null, string sourceType = null, string sourceName = null, string sourcePlayer = null, int npcLifePreHit = 0, int damageDealt = 0)
                {
                    NPCName = npcName;
                    SourceType = sourceType;
                    SourceName = sourceName;
                    SourcePlayer = sourcePlayer;
                    NPCLifePreHit = npcLifePreHit;
                    DamageDealt = damageDealt;
                }
                public string NPCName { get; set; }
                public string SourceType { get; set; }
                public string SourceName { get; set; }
                public string SourcePlayer { get; set; }
                public int NPCLifePreHit { get; set; }
                public int DamageDealt { get; set; }
            }

            public class ContextNPCKilled
            {
                public ContextNPCKilled(string npcname = null, string sourceWithPrefix = null, string sourceType = null, string sourceName = null, string sourcePlayer = null, int npcLifePreHit = 0, int damageDealt = 0, int overflowDamage = 0)
                {
                    NPCName = npcname;
                    SourceWithPrefix = sourceWithPrefix;
                    SourceType = sourceType;
                    SourceName = sourceName;
                    SourcePlayer = sourcePlayer;
                    NPCLifePreHit = npcLifePreHit;
                    DamageDealt = damageDealt;
                    OverflowDamage = overflowDamage;
                }
                public string NPCName { get; set; }
                public string SourceWithPrefix { get; set; }
                public string SourceType { get; set; }
                public string SourceName { get; set; }
                public string SourcePlayer { get; set; }
                public int NPCLifePreHit { get; set; }
                public int DamageDealt { get; set; }
                public int OverflowDamage { get; set; }
            }

            public class ContextPlayerKilled
            {
                public ContextPlayerKilled(string playerName = null, string sourceType = null, string sourceName = null, int playerLifePreHit = 0, int damageDealt = 0, int overflowDamage = 0, bool pvp = false)
                {
                    PlayerName = playerName;
                    SourceType = sourceType;
                    SourceName = sourceName;
                    PlayerLifePreHit = playerLifePreHit;
                    DamageDealt = damageDealt;
                    OverflowDamage = overflowDamage;
                    PVP = pvp;
                    if (sourceType == "PLAYER") PVP = true;
                }
                public string PlayerName { get; set; }
                public string SourceType { get; set; }
                public string SourceName { get; set; }
                public int PlayerLifePreHit { get; set; }
                public int DamageDealt { get; set; }
                public int OverflowDamage { get; set; }
                public bool PVP { get; set; }
            }

            public class ContextBossSpawn
            {
                public ContextBossSpawn(string name, int hp)
                {
                    Name = name;
                    Lifepoints = hp;
                }
                public string Name { get; set; }
                public int Lifepoints { get; set; }
            }
        }
    }
}