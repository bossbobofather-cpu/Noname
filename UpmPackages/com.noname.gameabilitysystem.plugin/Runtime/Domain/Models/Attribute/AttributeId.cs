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
        public static AttributeId MoveSpeed => new AttributeId("MoveSpeed");
        public static AttributeId JumpSpeed => new AttributeId("JumpSpeed");
        public static AttributeId Health => new AttributeId("Health");
        public static AttributeId AttackRange => new AttributeId("AttackRange");
        public static AttributeId AttackSpeed => new AttributeId("AttackSpeed");
        public static AttributeId AttackDamage => new AttributeId("AttackDamage");
        public static AttributeId MaxHealth => new AttributeId("MaxHealth");
        public static AttributeId Exp => new AttributeId("Exp");
        public static AttributeId Gold => new AttributeId("Gold");
    }
}
