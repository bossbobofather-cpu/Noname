namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 속성 식별자입니다 (순수 C# 구조체).
    /// </summary>
    public struct AttributeId
    {
        public string Name { get; set; }

        public AttributeId(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            return obj is AttributeId other && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return Name ?? string.Empty;
        }

        // 기본 속성 상수
        public static readonly AttributeId Level = new("Level");
        public static readonly AttributeId MoveSpeed = new AttributeId("MoveSpeed");
        public static readonly AttributeId JumpSpeed = new AttributeId("JumpSpeed");
        public static readonly AttributeId Health = new AttributeId("Health");
        public static readonly AttributeId AttackRange = new AttributeId("AttackRange");
        public static readonly AttributeId AttackSpeed = new AttributeId("AttackSpeed");
        public static readonly AttributeId AttackDamage = new AttributeId("AttackDamage");
        public static readonly AttributeId Defense = new("Defense");
        public static readonly AttributeId MaxHealth = new AttributeId("MaxHealth");
        public static readonly AttributeId ExtraTargetCount = new("ExtraTargetCount");
        public static readonly AttributeId Exp = new AttributeId("Exp");
        public static readonly AttributeId Gold = new AttributeId("Gold");
        public static readonly AttributeId Experience = new("Experience");
        public static readonly AttributeId ExpReward = new("ExpReward");
    }
}
