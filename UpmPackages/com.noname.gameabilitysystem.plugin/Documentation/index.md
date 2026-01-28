# GameAbilitySystem Plugin - API Documentation

## Overview

**GameAbilitySystem Plugin** is a Unity gameplay ability system inspired by Unreal Engine's Gameplay Ability System (GAS), reimagined with Clean Architecture principles and optimized for high-performance Unity environments.

### Key Features

- **Clean Architecture**: Strict Domain/Presentation separation with zero Unity dependencies in Domain layer
- **Thread-Safe**: Lock-based synchronization enables Host environment execution on separate threads
- **High Performance**: Zero-allocation design with struct-based tags and O(1) lookup performance
- **Extensible**: Tag-based activation system with custom calculators and async ability tasks
- **Production-Ready**: Battle-tested patterns from 10+ years of game development experience

---

## Architecture Layers

```
┌─────────────────────────────────────────┐
│        Presentation Layer               │
│    (Unity MonoBehaviour/ScriptableObject)│
│  • AbilitySystemComponent               │
│  • GameplayAbility                      │
│  • GameplayEffectConfig                 │
└──────────────┬──────────────────────────┘
               │ Bridge Layer
               │ (DomainConversionExtensions)
┌──────────────▼──────────────────────────┐
│         Domain Layer                    │
│       (Pure C#, Unity-Free)             │
│  • AbilitySystemModel                   │
│  • AttributeSetModel                    │
│  • GameplayTagContainerModel            │
│  ✅ Thread-Safe                         │
│  ✅ JSON Serializable                   │
│  ✅ .NET Standard 2.1                   │
└─────────────────────────────────────────┘
```

---

## Core Components

### Domain Layer (`Noname.GameAbilitySystem.Domain`)

The Domain layer contains pure C# business logic with **zero Unity dependencies**. All classes are thread-safe and can run in Host environments on separate threads.

#### AbilitySystemModel
Thread-safe state management for attributes, tags, and active gameplay effects.

**Key Methods:**
- `Set(AttributeId, float)` - Set attribute value (thread-safe)
- `AddTag(FGameplayTagModel)` - Add gameplay tag
- `AddActiveEffect(GameplayEffectModel, float)` - Apply gameplay effect
- `BuildSnapshot()` - Create immutable snapshot for rendering

#### AttributeSetModel
Manages character attributes (Health, Mana, Attack, etc.) with min/max clamping.

**Key Methods:**
- `SetAttribute(AttributeId, float, float, float)` - Initialize attribute
- `TryGet(AttributeId, out AttributeValueModel)` - Get attribute value
- `Modify(AttributeId, float, ModifierOperationType)` - Apply modifier

#### GameplayTagContainerModel
Hierarchical tag system with O(1) lookup performance using hash-based containers.

**Key Methods:**
- `HasTag(FGameplayTagModel)` - O(1) tag check
- `HasAll(IEnumerable<FGameplayTagModel>)` - Check multiple required tags
- `HasAny(IEnumerable<FGameplayTagModel>)` - Check if any tag matches

---

### Presentation Layer (`Noname.GameAbilitySystem.Presentation`)

Unity-specific components that bridge ScriptableObject workflows with the Domain layer.

#### AbilitySystemComponent
Main Unity component that manages ability lifecycle and effect application.

**Key Methods:**
- `GiveAbility(GameplayAbilityDefinition)` - Grant ability to character
- `TryActivateAbilityByType<T>()` - Activate ability by type
- `ApplyGameplayEffect(GameplayEffectConfig)` - Apply effect from config
- `HandleGameplayEvent(GameplayEventData)` - Process gameplay event

#### GameplayAbility
Abstract base class for implementing custom abilities with async task support.

**Key Methods:**
- `ActivateAbility(AbilityContext)` - Main activation logic (override required)
- `CanActivateAbility()` - Activation condition check
- `EndAbility(FGameplayAbilitySpecHandle)` - Graceful ability termination
- `CancelAbility(FGameplayAbilitySpecHandle)` - Forced cancellation

---

## Performance Characteristics

| Operation | Complexity | Notes |
|-----------|-----------|-------|
| Tag Lookup | O(1) | HashSet-based with cached hash codes |
| Attribute Modification | O(1) | Dictionary lookup |
| Effect Application | O(n) | n = number of modifiers in effect |
| Snapshot Creation | O(m) | m = total state size (full copy) |

### Memory Efficiency
- **Struct-based tags**: 16 bytes per FGameplayTag (string ref + int hash)
- **Zero allocation**: No GC pressure during gameplay
- **Hash caching**: Animator.StringToHash cached in struct

---

## Getting Started

### Quick Example

```csharp
using Noname.GameAbilitySystem.Presentation;
using UnityEngine;

public class MyFireballAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // 1. Get target
        var targetTask = AbilityTask_WaitTargetData.Create(this, targetConfig);
        targetTask.ValidData += OnTargetAcquired;
        targetTask.Activate();
    }

    private void OnTargetAcquired(AbilityTargetData data)
    {
        // 2. Apply damage (automatic from config)
        // GameplayEffectConfig is automatically converted to Domain model

        EndAbility(TaskOwner.Handle);
    }

    public override bool CanActivateAbility()
    {
        // Check mana cost
        return ASC.Attributes.TryGet(AttributeId.Mana, out var mana)
            && mana.CurrentValue >= 50f;
    }
}
```

---

## API Reference

Browse the complete API documentation:

- **[Domain Namespace](xref:Noname.GameAbilitySystem.Domain)** - Pure C# models and utilities
- **[Presentation Namespace](xref:Noname.GameAbilitySystem.Presentation)** - Unity components and configs

---

## Technical Specifications

- **Unity Version**: 6000.3.1f1 or higher
- **.NET Target**: .NET Standard 2.1
- **Threading**: Full thread-safety in Domain layer
- **Serialization**: JSON-compatible state snapshots
- **Dependencies**: Zero external dependencies

---

## Portfolio Highlights

This plugin demonstrates:

- ✅ **Clean Architecture**: Enforced dependency inversion with namespace separation
- ✅ **Multithreading**: Lock-based synchronization for Host/Client architecture
- ✅ **Performance Optimization**: Zero-allocation patterns and O(1) lookups
- ✅ **Scalable Design**: Tag-based system with async task support
- ✅ **Production Quality**: Comprehensive XML documentation and unit tests
- ✅ **10+ Years Experience**: Battle-tested architectural patterns

---

## Contact

For inquiries regarding this portfolio project, please refer to the project repository.
