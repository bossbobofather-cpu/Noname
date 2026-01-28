# Getting Started

## Installation

### Unity Package Manager (Local)

1. Open your Unity project
2. Open Package Manager (Window > Package Manager)
3. Click the **+** button and select **Add package from disk...**
4. Navigate to `UpmPackages/com.noname.gameabilitysystem.plugin/package.json`
5. Click **Open**

### Manual Installation

Copy the entire `com.noname.gameabilitysystem.plugin` folder to your project's `Packages/` directory.

---

## Your First Ability

### Step 1: Create Ability Class

Create a new C# script that inherits from `GameplayAbility`:

```csharp
using Noname.GameAbilitySystem.Presentation;
using UnityEngine;

public class BasicAttackAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        Debug.Log("Basic Attack activated!");

        // Apply damage effect (configured in ScriptableObject)
        // Effect is automatically applied based on GameplayEffectConfig

        EndAbility(context.Handle);
    }

    public override bool CanActivateAbility()
    {
        // Check if we can attack
        return !ASC.OwnedTags.HasTag(new FGameplayTag("Status.Stunned"));
    }
}
```

### Step 2: Create ScriptableObject Configs

#### 2.1 Create Gameplay Tag Config

1. Right-click in Project window
2. **Create > GameAbilitySystem > Config > Gameplay Tag Config**
3. Name it `TagConfig_BasicAttack`
4. Add Ability Tags: `Ability.Attack.Basic`

#### 2.2 Create Gameplay Effect Config

1. **Create > GameAbilitySystem > Config > Gameplay Effect Config**
2. Name it `Effect_BasicAttackDamage`
3. Configure:
   - **Duration Type**: Instant
   - **Modifiers**: Add one modifier
     - Attribute: Health
     - Operation: Add
     - Magnitude: -10

#### 2.3 Create Ability Definition

1. **Create > GameAbilitySystem > Ability Definition**
2. Name it `AbilityDef_BasicAttack`
3. Set **Ability Type Name**: `BasicAttackAbility`
4. Add configs to **Configs** array:
   - TagConfig_BasicAttack
   - Effect_BasicAttackDamage

### Step 3: Setup GameObject

Add the ability to your character:

```csharp
using Noname.GameAbilitySystem.Presentation;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private AbilitySystemComponent _abilitySystem;
    [SerializeField] private GameplayAbilityDefinition _basicAttackAbility;

    private void Start()
    {
        // Initialize attributes
        _abilitySystem.Attributes.SetAttribute(
            AttributeDefinition.Health,
            baseValue: 100f,
            minValue: 0f,
            maxValue: 100f
        );

        // Grant ability
        _abilitySystem.GiveAbility(_basicAttackAbility);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _abilitySystem.TryActivateAbilityByType<BasicAttackAbility>();
        }
    }
}
```

### Step 4: Assign Components

1. Add `AbilitySystemComponent` to your player GameObject
2. Add `PlayerController` script
3. Assign references in Inspector:
   - Ability System → AbilitySystemComponent
   - Basic Attack Ability → AbilityDef_BasicAttack

### Step 5: Test

Press Play and press **Space** to activate the ability!

---

## Advanced Example: Fireball with Target Acquisition

```csharp
using Noname.GameAbilitySystem.Presentation;
using UnityEngine;

public class FireballAbility : GameplayAbility
{
    [SerializeField] private TargetAcquisitionConfig _targetConfig;

    protected override void ActivateAbility(AbilityContext context)
    {
        // Wait for target selection
        var targetTask = AbilityTask_WaitTargetData.Create(this, _targetConfig);
        targetTask.ValidData += OnTargetAcquired;
        targetTask.Cancelled += () => CancelAbility(context.Handle);
        targetTask.Activate();
    }

    private void OnTargetAcquired(AbilityTargetData targetData)
    {
        // Get target's ability system
        var target = targetData.TargetActor.GetComponent<AbilitySystemComponent>();

        if (target != null)
        {
            // Apply damage effect
            target.ApplyGameplayEffect(damageEffect);

            // Apply burning DoT
            target.ApplyGameplayEffect(burningEffect);
        }

        EndAbility(TaskOwner.Handle);
    }

    public override bool CanActivateAbility()
    {
        // Check mana cost
        if (!ASC.Attributes.TryGet(AttributeId.Mana, out var mana))
            return false;

        return mana.CurrentValue >= 50f;
    }
}
```

---

## Key Concepts

### Attributes

Attributes are numeric values like Health, Mana, Attack Power, etc.

```csharp
// Set attribute
ASC.Attributes.SetAttribute(AttributeId.Health, 100f, 0f, 100f);

// Get attribute
if (ASC.Attributes.TryGet(AttributeId.Health, out var health))
{
    Debug.Log($"Health: {health.CurrentValue}/{health.MaxValue}");
}

// Modify attribute
ASC.Attributes.Modify(AttributeId.Health, -10f, ModifierOperationType.Add);
```

### Gameplay Tags

Hierarchical tags for activation conditions and state tracking.

```csharp
// Add tag
ASC.OwnedTags.AddTag(new FGameplayTag("Status.Poisoned"));

// Check tag
if (ASC.OwnedTags.HasTag(new FGameplayTag("Status.Poisoned")))
{
    Debug.Log("Character is poisoned!");
}

// Check parent tag (hierarchical)
// This checks for ANY tag starting with "Status."
if (ASC.OwnedTags.HasTag(new FGameplayTag("Status")))
{
    Debug.Log("Character has some status effect!");
}
```

### Gameplay Effects

Effects modify attributes over time with various duration types.

```csharp
// Instant effect (damage)
var damageEffect = ScriptableObject.CreateInstance<GameplayEffectConfig>();
damageEffect.DurationType = EGameplayEffectDurationType.Instant;
ASC.ApplyGameplayEffect(damageEffect);

// Duration effect (temporary buff)
var buffEffect = ScriptableObject.CreateInstance<GameplayEffectConfig>();
buffEffect.DurationType = EGameplayEffectDurationType.HasDuration;
buffEffect.Duration = 10f; // 10 seconds
ASC.ApplyGameplayEffect(buffEffect);

// Infinite effect (permanent until removed)
var permBuffEffect = ScriptableObject.CreateInstance<GameplayEffectConfig>();
permBuffEffect.DurationType = EGameplayEffectDurationType.Infinite;
var handle = ASC.ApplyGameplayEffect(permBuffEffect);

// Remove effect manually
ASC.RemoveActiveEffect(handle);
```

---

## Next Steps

- [Architecture Guide](architecture.md) - Understand the Clean Architecture design
- [Performance Guide](performance.md) - Optimization techniques and best practices
- [API Reference](../obj/api/index.md) - Complete API documentation

---

## Common Issues

### Ability Not Activating

1. Check `CanActivateAbility()` returns true
2. Verify Required Tags are owned
3. Ensure Blocked Tags are NOT owned
4. Check console for activation failure logs

### Effect Not Applying

1. Verify target has `AbilitySystemComponent`
2. Check effect's Application Tag Requirements
3. Ensure Immunity Tags don't block the effect
4. Check attribute exists on target

### Missing References

1. Ensure all ScriptableObject configs are assigned
2. Check Ability Definition has correct Type Name
3. Verify AbilitySystemComponent is initialized
