using UnityEngine;
using ThunderRoad;
using XenobladeRPG;

namespace NewMonado
{
    public class XCRPG : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<XCRPGComponent>();
        }
    }
    public class XCRPGComponent : MonoBehaviour 
    {
        Item item;
        public MonadoComponent monado;
        public void Start()
        {
            item = GetComponent<Item>();
            monado = GetComponent<MonadoComponent>();
            XenobladeEvents.onXenobladeDamage += XenobladeEvents_onXenobladeDamage; 
        }

        private void XenobladeEvents_onXenobladeDamage(ref CollisionInstance collisionInstance, ref Creature attacker, ref Creature defender, ref XenobladeDamageType damageType, EventTime eventTime)
        {
            if ((collisionInstance?.sourceCollider?.gameObject == monado.beam || collisionInstance?.targetCollider?.gameObject == monado.beam) && eventTime == EventTime.OnEnd)
            {
                if (defender != attacker && defender != item?.mainHandler?.creature && !defender.isKilled && collisionInstance.damageStruct.damage >= 1)
                    switch (monado.component)
                    {
                        case "Cyclone":
                            Destroy(defender.gameObject.GetComponent<XCRPGCycloneAura>());
                            defender.gameObject.AddComponent<XCRPGCycloneAura>();
                            break;
                        case "Purge":
                            if (defender.gameObject.GetComponent<XCRPGPurgeAura>() != null) Destroy(defender.gameObject.GetComponent<XCRPGPurgeAura>());
                            defender.gameObject.AddComponent<XCRPGPurgeAura>().Setup(monado.PurgeAuraTime);
                            if (collisionInstance.targetColliderGroup != null)
                            {
                                monado.ActivateAura("MonadoPurge");
                                monado.SwapAbility("Default");
                            }
                            break;
                        case "Eater":
                            Destroy(defender.gameObject.GetComponent<XCRPGEaterAura>());
                            defender.gameObject.AddComponent<XCRPGEaterAura>().Setup(collisionInstance);
                            if (collisionInstance.targetColliderGroup != null)
                            {
                                monado.ActivateAura("MonadoEater");
                                monado.SwapAbility("Default");
                            }
                            break;
                        default:
                            break;
                    }
            }
        }
        public void FixedUpdate()
        {
            if (item.IsHanded() && monado.triggerPressed && (item.rb.velocity - item.mainHandler.creature.currentLocomotion.rb.velocity).sqrMagnitude >= 225f)
            {
                switch (monado.component)
                {
                    case "Enchant":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId == item.mainHandler.creature.factionId && creature != item.mainHandler.creature && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<XCRPGEnchantAura>());
                                creature.gameObject.AddComponent<XCRPGEnchantAura>().Setup(monado.EnchantAuraTime);
                            }
                        }
                        monado.ActivateAura("MonadoEnchant");
                        monado.SwapAbility("Default");
                        break;
                    case "Shield":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId == item.mainHandler.creature.factionId && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<XCRPGShieldAura>());
                                creature.gameObject.AddComponent<XCRPGShieldAura>().Setup(monado.ShieldAuraTime, monado.ShieldDamageAbsorb);
                            }
                        }
                        monado.ActivateAura("MonadoShield");
                        monado.SwapAbility("Default");
                        break;
                    case "Speed":
                        Destroy(item.mainHandler.creature.gameObject.GetComponent<XCRPGSpeedAura>());
                        item.mainHandler.creature.gameObject.AddComponent<XCRPGSpeedAura>().Setup(monado.SpeedAuraTime, monado.SpeedMultiplier);
                        monado.ActivateAura("MonadoSpeed");
                        monado.SwapAbility("Default");
                        break;
                    case "Cyclone":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature != item.mainHandler.creature && creature.factionId != item.mainHandler.creature.factionId && creature.isActiveAndEnabled && !creature.isKilled &&
                                Vector3.Distance(item.mainHandler.creature.transform.position, creature.transform.position) <= 25)
                            {
                                CollisionInstance wave = new CollisionInstance(new DamageStruct(DamageType.Energy, 20))
                                {
                                    sourceCollider = monado.beam.GetComponent<Collider>(),
                                    sourceColliderGroup = item.colliderGroups[0]
                                };
                                wave.damageStruct.hitRagdollPart = creature.ragdoll.rootPart;
                                creature.Damage(wave);
                            }
                        }
                        monado.ActivateAura("MonadoCyclone");
                        monado.SwapAbility("Default");
                        break;
                    case "Armor":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId == item.mainHandler.creature.factionId && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<XCRPGArmorAura>());
                                creature.gameObject.AddComponent<XCRPGArmorAura>().Setup(monado.ArmorAuraTime, monado.ArmorDamageReductionMult);
                            }
                        }
                        monado.ActivateAura("MonadoArmor");
                        monado.SwapAbility("Default");
                        break;
                    default:
                        break;
                }
            }
            if (item.IsHanded() && monado.triggerPressed && item.rb.GetPointVelocity(item.flyDirRef.position).magnitude - item.rb.GetPointVelocity(item.holderPoint.position).magnitude >= 10)
            {
                if (monado.component == "Purge" || monado.component == "Eater")
                {
                    foreach (Creature creature in Creature.allActive)
                    {
                        if (creature != null && creature != item?.mainHandler?.creature && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && creature.faction != item.mainHandler.creature.faction &&
                            Vector3.Angle(item.mainHandler.creature.centerEyes.forward, (creature.ragdoll.rootPart.transform.position - item.mainHandler.creature.transform.position).normalized) <= 60 &&
                            Vector3.Distance(item.mainHandler.creature.transform.position, creature.transform.position) <= 25)
                        {
                            CollisionInstance wave = new CollisionInstance(new DamageStruct(DamageType.Energy, 20))
                            {
                                sourceCollider = monado.beam.GetComponent<Collider>(),
                                sourceColliderGroup = item.colliderGroups[0]
                            };
                            wave.damageStruct.hitRagdollPart = creature.ragdoll.rootPart;
                            creature.Damage(wave);
                        }
                    }
                    if (monado.component == "Purge")
                        monado.ActivateAura("MonadoPurge");
                    else if (monado.component == "Eater")
                        monado.ActivateAura("MonadoEater");
                    EffectInstance effectInstance = Catalog.GetData<EffectData>("MonadoWave").Spawn(monado.slash.transform.position, item.mainHandler.creature.centerEyes.rotation);
                    effectInstance.SetIntensity(1.0f);
                    effectInstance.Play();
                    monado.SwapAbility("Default");
                }
            }
        }
    }
    public class XCRPGEnchantAura : MonoBehaviour
    {
        Creature creature;
        Item rightItem;
        Item leftItem;
        SpellCastCharge imbueSpell;
        float timer;
        EffectInstance instance;
        float auraTime;
        public void Setup(float time)
        {
            auraTime = time;
        }
        public void Start()
        {
            creature = GetComponent<Creature>();
            imbueSpell = Catalog.GetData<SpellCastCharge>("Fire");
            timer = Time.time;
            instance = Catalog.GetData<EffectData>("MonadoEnchantAura").Spawn(creature.transform);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
            XenobladeEvents.InvokeOnBuffAdded(ref creature, this);
        }
        public void FixedUpdate()
        {
            if (Time.time - timer >= auraTime || creature.isKilled)
            {
                if (creature.handRight.grabbedHandle?.item != null)
                {
                    rightItem = creature.handRight.grabbedHandle.item;
                    foreach (Imbue imbue in rightItem.imbues)
                    {
                        imbue.Stop();
                    }
                }
                if (creature.handLeft.grabbedHandle?.item != null)
                {
                    leftItem = creature.handLeft.grabbedHandle.item;
                    foreach (Imbue imbue in leftItem.imbues)
                    {
                        imbue.Stop();
                    }
                }
                instance.Stop();
                Destroy(this);
            }
            else
            {
                if (creature.handRight.grabbedHandle?.item != null)
                {
                    rightItem = creature.handRight.grabbedHandle.item;
                    foreach (Imbue imbue in rightItem.imbues)
                    {
                        imbue.Transfer(imbueSpell, 5f);
                    }
                }
                if (creature.handLeft.grabbedHandle?.item != null)
                {
                    leftItem = creature.handLeft.grabbedHandle.item;
                    foreach (Imbue imbue in leftItem.imbues)
                    {
                        imbue.Transfer(imbueSpell, 5f);
                    }
                }
            }
        }
        public void OnDestroy()
        {
            XenobladeEvents.InvokeOnBuffRemoved(ref creature, this);
        }
    }
    public class XCRPGShieldAura : MonoBehaviour
    {
        Creature creature;
        float timer;
        EffectInstance instance;
        float auraTime;
        public float absorb;
        public void Setup(float time, float damageAbsorb)
        {
            auraTime = time;
            absorb = damageAbsorb;
        }
        public void Start()
        {
            creature = GetComponent<Creature>();
            timer = Time.time;
            instance = Catalog.GetData<EffectData>("MonadoShieldAura").Spawn(creature.transform);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
            XenobladeEvents.InvokeOnBuffAdded(ref creature, this);
        }
        public void FixedUpdate()
        {
            if (Time.time - timer >= auraTime || creature.isKilled || absorb <= 0)
            {
                Destroy(this);
            }
        }
        public void OnDestroy()
        {
            instance.Stop();
            XenobladeEvents.InvokeOnBuffRemoved(ref creature, this);
        }
    }
    public class XCRPGSpeedAura : MonoBehaviour
    {
        Creature creature;
        float timer;
        EffectInstance instance;
        float auraTime;
        float mult;
        XenobladeStats stats;
        public void Setup(float time, float multiplier)
        {
            auraTime = time;
            mult = multiplier;
        }
        public void Start()
        {
            creature = GetComponent<Creature>();
            stats = creature.GetComponent<XenobladeStats>();
            timer = Time.time;
            creature.currentLocomotion.SetSpeedModifier(this, mult, mult, mult, mult, mult);
            if (creature.isPlayer)
                XenobladeManager.SetStatModifier(this, 1, 1, 1, 1, 1, 0, 0, 0, 0, 50, 0, 0);
            else
                stats.SetStatModifier(this, 1, 1, 1, 1, 0, 0, 50, 0);
            instance = Catalog.GetData<EffectData>("MonadoSpeedAura").Spawn(creature.transform);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
            XenobladeEvents.InvokeOnBuffAdded(ref creature, this, XenobladeManager.statModifiers.Find(match => match.handler == this), stats.statModifiers.Find(match => match.handler == this));
        }
        public void FixedUpdate()
        {
            if (Time.time - timer >= auraTime || creature.isKilled)
            {
                Destroy(this);
            }
        }
        public void OnDestroy()
        {
            if (creature.isPlayer)
                XenobladeManager.RemoveStatModifier(this);
            else
                creature.GetComponent<XenobladeStats>().RemoveStatModifier(this);
            creature.currentLocomotion.RemoveSpeedModifier(this);
            instance?.Stop();
            XenobladeEvents.InvokeOnBuffRemoved(ref creature, this);
        }
    }
    public class XCRPGPurgeAura : MonoBehaviour
    {
        Creature creature;
        public float timer;
        public float auraTime;
        XenobladeStats stats;
        public void Setup(float time)
        {
            auraTime = time;
        }
        public void Start()
        {
            creature = GetComponent<Creature>();
            timer = Time.time;
            stats = creature.GetComponent<XenobladeStats>();
            XenobladeEvents.InvokeOnDebuffAdded(ref creature, this);
            creature.TryElectrocute(1, 3, true, false, Catalog.GetData<EffectData>("ImbueLightningRagdoll", true));
            creature.ragdoll.AddPhysicToggleModifier(this);
            if (stats != null) stats.isAuraSealed = true;
            else XenobladeManager.isAuraSealed = true;
        }

        public void FixedUpdate()
        {
            if (Time.time - timer >= auraTime || creature.isKilled)
            {
                Destroy(this);
            }
            else
            {
                if (stats != null) stats.isAuraSealed = true;
                else XenobladeManager.isAuraSealed = true;
                creature.mana.currentMana = 0f;
                Destroy(creature.gameObject.GetComponent<EnchantAura>());
                Destroy(creature.gameObject.GetComponent<ShieldAura>());
                Destroy(creature.gameObject.GetComponent<SpeedAura>());
                Destroy(creature.gameObject.GetComponent<ArmorAura>());
            }
        }
        public void OnDestroy()
        {
            creature.ragdoll.RemovePhysicToggleModifier(this);
            if (stats != null) stats.isAuraSealed = false;
            else XenobladeManager.isAuraSealed = false;
            creature.mana.currentMana = creature.mana.maxMana;
            XenobladeEvents.InvokeOnDebuffRemoved(ref creature, this);
        }
    }
    public class XCRPGCycloneAura : MonoBehaviour
    {
        Creature creature;
        float timer;
        public void Start()
        {
            creature = GetComponent<Creature>();
            timer = Time.time;
            creature.ragdoll.SetState(Ragdoll.State.Destabilized);
            creature.brain.AddNoStandUpModifier(this);
        }

        public void FixedUpdate()
        {
            if (Time.time - timer >= 3 || creature.isKilled)
            {
                Destroy(this);
            }
        }
        public void OnDestroy()
        {
            creature.brain.RemoveNoStandUpModifier(this);
        }
    }
    public class XCRPGEaterAura : MonoBehaviour
    {
        Creature creature;
        CollisionInstance collisionInstance;
        public void Start()
        {
            creature = GetComponent<Creature>();
            XenobladeEvents.InvokeOnDebuffAdded(ref creature, this);
            creature.TryPush(Creature.PushType.Hit, (creature.transform.position - collisionInstance.sourceColliderGroup.collisionHandler.item.lastHandler.creature.transform.position).normalized, 1, collisionInstance.damageStruct.hitRagdollPart.type);
            Destroy(creature.gameObject.GetComponent<EnchantAura>());
            Destroy(creature.gameObject.GetComponent<ShieldAura>());
            Destroy(creature.gameObject.GetComponent<SpeedAura>());
            Destroy(creature.gameObject.GetComponent<ArmorAura>());
            XenobladeStats stats = creature.GetComponent<XenobladeStats>();
            if (stats != null)
                for (int index = 0; index < stats.statModifiers.Count; ++index)
                {
                    if (stats.statModifiers[index] != null && (stats.statModifiers[index].strengthMultiplier > 1 || stats.statModifiers[index].etherMultiplier > 1 || stats.statModifiers[index].physicalDefenseModifier > 0
                        || stats.statModifiers[index].etherDefenseModifier > 0 || stats.statModifiers[index].agilityMultiplier > 1))
                        stats.RemoveStatModifier(stats.statModifiers[index].handler);
                }
            if (creature.GetComponent<Bleed>() is Bleed bleed)
            {
                if (bleed.initialDamage.damageStruct.damage > collisionInstance.damageStruct.damage)
                {
                    Destroy(bleed);
                    creature.gameObject.AddComponent<Bleed>().initialDamage = collisionInstance;
                }
                else bleed.time = Time.time;
            }
            else creature.gameObject.AddComponent<Bleed>().initialDamage = collisionInstance;
            Destroy(this);
        }
        public void Setup(CollisionInstance instance)
        {
            collisionInstance = instance;
        }
        public void OnDestroy()
        {
            XenobladeEvents.InvokeOnDebuffRemoved(ref creature, this);
        }
    }
    public class XCRPGArmorAura : MonoBehaviour
    {
        Creature creature;
        float timer;
        EffectInstance instance;
        float auraTime;
        public float mult;
        public void Setup(float time, float multiplier)
        {
            auraTime = time;
            mult = multiplier;
            if (mult > 1) mult = 1;
            else if (mult < 0) mult = 0;
        }
        public void Start()
        {
            creature = GetComponent<Creature>();
            timer = Time.time;
            instance = Catalog.GetData<EffectData>("MonadoArmorAura").Spawn(creature.transform);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
            XenobladeEvents.InvokeOnBuffAdded(ref creature, this);
        }
        public void FixedUpdate()
        {
            if (Time.time - timer >= auraTime || creature.isKilled)
            {
                Destroy(this);
            }
        }
        public void OnDestroy()
        {
            instance.Stop();
            XenobladeEvents.InvokeOnBuffRemoved(ref creature, this);
        }
    }
}
