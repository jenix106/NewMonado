using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using UnityEngine.VFX;

namespace NewMonado
{
    public class MonadoSpell : SpellCastData
    {
        SpellCaster spellCaster;
        MonadoComponent monadoComponent;
        public override void Fire(bool active)
        {
            base.Fire(active);
            if (active)
            {
                Fire(false);
            }
        }
        public override void Load(SpellCaster spellCaster, Level level)
        {
            base.Load(spellCaster, level);
            this.spellCaster = spellCaster;
            if (spellCaster.ragdollHand.grabbedHandle?.item is Item item && item.GetComponent<MonadoComponent>() is MonadoComponent monado && monado.active)
            {
                monadoComponent = monado;
                monadoComponent.SwapAbility(id);
            }
            else spellCaster.UnloadSpell();
        }

        public override void Unload()
        {
            base.Unload();
            if (monadoComponent != null && monadoComponent.active && monadoComponent.GetComponent<Item>().handlers.Contains(spellCaster.ragdollHand))
            {
                monadoComponent.SwapAbility("Default");
            }
        }
    }
    public class MonadoModule : ItemModule
    {
        public bool Voices;
        public float BusterDamage;
        public float EnchantAuraTime;
        public float ShieldAuraTime;
        public float ShieldDamageAbsorb;
        public float SpeedAuraTime;
        public float SpeedMultiplier;
        public float PurgeAuraTime;
        public float CycloneAuraTime;
        public float ArmorAuraTime;
        public float ArmorDamageReductionMult;
        public float EaterAuraTime;
        public float EaterDamageOverTime;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<MonadoComponent>().Setup(Voices, BusterDamage, EnchantAuraTime, ShieldAuraTime, ShieldDamageAbsorb, SpeedAuraTime, SpeedMultiplier, PurgeAuraTime, CycloneAuraTime, ArmorAuraTime, ArmorDamageReductionMult, EaterAuraTime, EaterDamageOverTime);
        }
    }
    public class MonadoComponent : MonoBehaviour
    {
        Item item;
        public Damager slash;
        public Damager pierce;
        public GameObject center;
        public GameObject defaultSymbol;
        public GameObject busterSymbol;
        public GameObject enchantSymbol;
        public GameObject shieldSymbol;
        public GameObject speedSymbol;
        public GameObject purgeSymbol;
        public GameObject cycloneSymbol;
        public GameObject armorSymbol;
        public GameObject eaterSymbol;
        public GameObject beam;
        public GameObject beamCross;
        public GameObject cycloneFX;
        public GameObject ringDefault;
        public GameObject ringAbility;
        public Animation animator;
        public string component;
        public Vector3 original;
        public bool active = false;
        public string voiceText = "";
        public bool Voices;
        public bool triggerPressed = false;
        public bool wasGrabbed = false;
        public float BusterDamage;
        public float EnchantAuraTime;
        public float ShieldAuraTime;
        public float ShieldDamageAbsorb;
        public float SpeedAuraTime;
        public float SpeedMultiplier;
        public float PurgeAuraTime;
        public float CycloneAuraTime;
        public float ArmorAuraTime;
        public float ArmorDamageReductionMult;
        public float EaterAuraTime;
        public float EaterDamageOverTime;
        public ContainerData monadoSpells;
        public ContainerData otherSpells;
        public EffectInstance voice = new EffectInstance();
        public XCRPGComponent xenobladeRPG;
        public void Start()
        {
            item = GetComponent<Item>();
            slash = item.GetCustomReference("Slash").GetComponent<Damager>();
            pierce = item.GetCustomReference("Pierce").GetComponent<Damager>();
            center = item.GetCustomReference("Center").gameObject;
            defaultSymbol = item.GetCustomReference("Default").gameObject;
            busterSymbol = item.GetCustomReference("Buster").gameObject;
            enchantSymbol = item.GetCustomReference("Enchant").gameObject;
            shieldSymbol = item.GetCustomReference("Shield").gameObject;
            speedSymbol = item.GetCustomReference("Speed").gameObject;
            purgeSymbol = item.GetCustomReference("Purge").gameObject;
            cycloneSymbol = item.GetCustomReference("Cyclone").gameObject;
            armorSymbol = item.GetCustomReference("Armor").gameObject;
            eaterSymbol = item.GetCustomReference("Eater").gameObject;
            if (item.data.id != "NewMonadoIII")
            {
                ringDefault = item.GetCustomReference("RingDefault")?.gameObject;
                ringAbility = item.GetCustomReference("RingAbility")?.gameObject;
            }
            beam = item.GetCustomReference("Beam").gameObject;
            beamCross = item.GetCustomReference("BeamCross").gameObject;
            cycloneFX = item.GetCustomReference("CycloneFX").gameObject;
            animator = item.gameObject.GetComponent<Animation>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnSnapEvent += Item_OnSnapEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
            original = beam.transform.localPosition;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            animator.Play("Deactivate");
            foreach (MaterialData data in Catalog.GetDataList(Category.Material))
            {
                if (!slash.data.damageModifierData.collisions[0].targetMaterials.Contains(data)) slash.data.damageModifierData.collisions[0].targetMaterials.Add(data);
                if (!pierce.data.damageModifierData.collisions[0].targetMaterials.Contains(data)) pierce.data.damageModifierData.collisions[0].targetMaterials.Add(data);
            }
            monadoSpells = Catalog.GetData<ContainerData>("MonadoSpells");
            otherSpells = Catalog.GetData<ContainerData>("PlayerDefault");
            xenobladeRPG = GetComponent<XCRPGComponent>();
        }

        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            ragdollHand.caster.UnloadSpell();
            if (!item.handlers.Contains(ragdollHand.otherHand))
            {
                SwapAbility("Default");
                foreach (ContainerData.Content spell in otherSpells.contents)
                {
                    if (spell != null && spell.itemData?.GetModule<ItemModuleSpell>() != null && !Player.local.creature.container.contents.Exists(match => match.itemData == spell.itemData))
                    {
                        Player.local.creature.container.contents.Add(spell);
                    }
                }
                foreach (ContainerData.Content spell in monadoSpells.contents)
                {
                    if (spell != null && Player.local.creature.container.contents.Exists(match => match.itemData == spell.itemData))
                    {
                        Player.local.creature.container.RemoveContent(spell);
                    }
                }
            }
        }

        public void Setup(bool voices, float busterDamage, float enchantTime, float shieldTime, float shieldAbsorb, float speedTime, float speedMult, float purgeTime, float cycloneTime, float armorTime, float armorMult, float eaterTime, float eaterDOT)
        {
            Voices = voices;
            BusterDamage = busterDamage;
            EnchantAuraTime = enchantTime;
            ShieldAuraTime = shieldTime;
            ShieldDamageAbsorb = shieldAbsorb;
            SpeedAuraTime = speedTime;
            SpeedMultiplier = speedMult;
            PurgeAuraTime = purgeTime;
            CycloneAuraTime = cycloneTime;
            ArmorAuraTime = armorTime;
            ArmorDamageReductionMult = armorMult;
            EaterAuraTime = eaterTime;
            EaterDamageOverTime = eaterDOT;
            if (Voices) voiceText = "Voice";
        }

        private void Item_OnSnapEvent(Holder holder)
        {
            SwapAbility("Default");
            if (active) ToggleBlade();
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (collisionInstance.sourceCollider.gameObject == beam || collisionInstance.targetCollider.gameObject == beam)
            {
                if (collisionInstance.targetCollider.GetComponentInParent<Creature>() is Creature creature && creature != Player.local.creature && !creature.isKilled && collisionInstance.damageStruct.damage >= 1 && xenobladeRPG == null)
                    switch (component)
                    {
                        case "Purge":
                            Destroy(creature.gameObject.GetComponent<PurgeAura>());
                            creature.gameObject.AddComponent<PurgeAura>().Setup(PurgeAuraTime);
                            ActivateAura("MonadoPurge");
                            SwapAbility("Default");
                            break;
                        case "Eater":
                            Destroy(creature.gameObject.GetComponent<EaterAura>());
                            creature.gameObject.AddComponent<EaterAura>().Setup(EaterAuraTime, EaterDamageOverTime);
                            ActivateAura("MonadoEater");
                            SwapAbility("Default");
                            break;
                        default:
                            break;
                    }
                if (component == "Buster" && collisionInstance.sourceCollider.GetComponentInParent<Creature>() != Player.local.creature)
                {
                    if (item.isPenetrating && collisionInstance.sourceColliderGroup.GetComponentInParent<Creature>() != null) StartCoroutine(SliceAll(collisionInstance.sourceColliderGroup.GetComponentInParent<Creature>()));
                    StartCoroutine(Buster(collisionInstance.contactPoint, collisionInstance.contactNormal, collisionInstance.sourceColliderGroup.transform.up));
                    ActivateAura("MonadoBuster");
                    SwapAbility("Default");
                }
            }
        }
        private IEnumerator SliceAll(Creature creature)
        {
            foreach (RagdollPart part in creature.ragdoll.parts)
            {
                if (part.sliceAllowed)
                {
                    yield return null;
                    part.ragdoll.TrySlice(part);
                    if (part.data.sliceForceKill) part.ragdoll.creature.Kill();
                }
            }
        }
        private IEnumerator Buster(Vector3 contactPoint, Vector3 contactNormal, Vector3 contactNormalUpward)
        {
            EffectInstance effectInstance = Catalog.GetData<EffectData>("SpellGravityShockwave").Spawn(contactPoint, Quaternion.LookRotation(-contactNormal, contactNormalUpward), null, null, false);
            effectInstance.SetIntensity(15);
            effectInstance.Play();
            Collider[] sphereContacts = Physics.OverlapSphere(contactPoint, 15, 218119169);
            List<Creature> creaturesPushed = new List<Creature>();
            List<Rigidbody> rigidbodiesPushed = new List<Rigidbody>();
            rigidbodiesPushed.Add(item.physicBody.rigidBody);
            if (item.lastHandler?.creature)
                creaturesPushed.Add(item.lastHandler.creature);
            float waveDistance = 0.0f;
            while (waveDistance < 15)
            {
                waveDistance += 20f * 0.05f;
                foreach (Creature creature in Creature.allActive)
                {
                    if (!creature.isKilled && Vector3.Distance(contactPoint, creature.transform.position) < waveDistance && !creaturesPushed.Contains(creature) && creature.faction != Player.local.creature.faction)
                    {
                        CollisionInstance collision = new CollisionInstance(new DamageStruct(DamageType.Energy, BusterDamage - (Vector3.Distance(contactPoint, creature.transform.position) * 2)));
                        collision.damageStruct.hitRagdollPart = creature.ragdoll.rootPart;
                        creature.Damage(collision);
                        if (!creature.isPlayer)
                            creature.TryPush(Creature.PushType.Magic, (creature.ragdoll.rootPart.transform.position - contactPoint).normalized, 3);
                        if (item?.lastHandler?.creature != null)
                        {
                            creature.lastInteractionTime = Time.time;
                            creature.lastInteractionCreature = item.lastHandler.creature;
                        }
                        creaturesPushed.Add(creature);
                    }
                }
                foreach (Collider collider in sphereContacts)
                {
                    Breakable breakable = collider.attachedRigidbody?.GetComponentInParent<Breakable>();
                    if (breakable != null)
                    {
                        if (20 * 20 > breakable.instantaneousBreakVelocityThreshold)
                            breakable.Break();
                        for (int index = 0; index < breakable.subBrokenItems.Count; ++index)
                        {
                            Rigidbody rigidBody = breakable.subBrokenItems[index].physicBody.rigidBody;
                            if (rigidBody && !rigidbodiesPushed.Contains(rigidBody))
                            {
                                rigidBody.AddExplosionForce(20, contactPoint, 15, 0.5f, ForceMode.VelocityChange);
                                rigidbodiesPushed.Add(rigidBody);
                            }
                        }
                        for (int index = 0; index < breakable.subBrokenBodies.Count; ++index)
                        {
                            PhysicBody subBrokenBody = breakable.subBrokenBodies[index];
                            if (subBrokenBody && !rigidbodiesPushed.Contains(subBrokenBody.rigidBody))
                            {
                                subBrokenBody.rigidBody.AddExplosionForce(20, contactPoint, 15, 0.5f, ForceMode.VelocityChange);
                                rigidbodiesPushed.Add(subBrokenBody.rigidBody);
                            }
                        }
                    }
                    if (collider.attachedRigidbody != null && !collider.attachedRigidbody.isKinematic && Vector3.Distance(contactPoint, collider.transform.position) < waveDistance)
                    {
                        if (collider.attachedRigidbody.gameObject.layer != GameManager.GetLayer(LayerName.NPC) && !rigidbodiesPushed.Contains(collider.attachedRigidbody))
                        {
                            if(collider.attachedRigidbody.GetComponentInParent<Creature>() == null || collider.attachedRigidbody.GetComponentInParent<Creature>().faction != Player.local.creature.faction)
                            collider.attachedRigidbody.AddExplosionForce(20, contactPoint, 15, 0.5f, ForceMode.VelocityChange);
                            rigidbodiesPushed.Add(collider.attachedRigidbody);
                        }
                    }
                }
                yield return new WaitForSeconds(0.05f);
            }
            slash.UnPenetrateAll();
            pierce.UnPenetrateAll();
        }

        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            wasGrabbed = true;
            if (ragdollHand.creature.data.id == "Shulk")
            {
                SwapAbility("Default");
                if (!active) ToggleBlade();
            }
            else if (ragdollHand.creature != Player.local.creature && ragdollHand.creature.faction != Player.local.creature.faction)
            {
                if (!active) ToggleBlade();
                Creature target = ragdollHand.GetComponentInParent<Creature>();
                target.TryElectrocute(1, 3, true, false, Catalog.GetData<EffectData>("ImbueLightningRagdoll", true));
                ragdollHand.UnGrab(false);
            }
            if (ragdollHand.creature.isPlayer)
            {
                AddMonadoSpells(Player.local.creature.container);
            }
        }
        public void AddMonadoSpells(Container container)
        {
            foreach (ContainerData.Content content in monadoSpells.contents)
            {
                if (!container.contents.Contains(content))
                    container.contents.Add(content);
            }
            foreach (ContainerData.Content spell in otherSpells.contents)
            {
                ItemModuleSpell module = spell.itemData.GetModule<ItemModuleSpell>();
                if (module != null && module.spellData is SpellCastData && string.IsNullOrEmpty(spell.itemData.category) && !monadoSpells.contents.Contains(spell) && spell.itemData.iconEffectData != null)
                {
                    container.RemoveContent(spell.itemData.id);
                }
            }
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (((action == Interactable.Action.AlternateUseStart) && PlayerControl.GetHand(ragdollHand.side).castPressed) ||
                (action == Interactable.Action.UseStart && PlayerControl.GetHand(ragdollHand.side).alternateUsePressed))
            {
                PlayerControl.GetHand(ragdollHand.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected);
                ToggleBlade();
                SwapAbility("Default");
                if (!active && item.isPenetrating)
                {
                    pierce.UnPenetrateAll();
                    slash.UnPenetrateAll();
                }
                if (!active)
                {
                    ringAbility?.SetActive(false);
                    ringDefault?.SetActive(true);
                }
            }
            if (action == Interactable.Action.UseStart) triggerPressed = true;
            else if (action == Interactable.Action.UseStop) triggerPressed = false;
        }
        public void ToggleBlade()
        {
            if (!active)
            {
                animator.Play("Activate");
                if (item.data.id != "NewMonadoIII")
                {
                    foreach (ColliderGroup group in item.colliderGroups)
                    {
                        if (group.name.Equals("Front") || group.name.Equals("Back"))
                            group.imbueEffectRenderer.gameObject.AddComponent<ShockEffect>();
                    }
                }
            }
            else if (active)
            {
                animator.Play("Deactivate");
                if (item.data.id != "NewMonadoIII")
                {
                    foreach (ColliderGroup group in item.colliderGroups)
                    {
                        if (group.name.Equals("Front") || group.name.Equals("Back"))
                            Destroy(group.imbueEffectRenderer.gameObject.GetComponent<ShockEffect>());
                    }
                }
                foreach(Imbue imbue in item.imbues)
                {
                    imbue.Stop();
                }
            }
            active = !active;
        }
        public class ShockEffect : MonoBehaviour
        {
            EffectInstance instance;
            public void Start()
            {
                instance = Catalog.GetData<EffectData>("MonadoElectricity").Spawn(gameObject.transform, null, false);
                instance.SetRenderer(gameObject.GetComponent<Renderer>(), false);
                instance.SetIntensity(0.1f);
                instance.Play();
            }
            public void OnDestroy()
            {
                instance?.Stop();
            }
        }
        public void SetDefault()
        {
            Color defaultBlade = new Color(1, 1, 1, 1);
            Color defaultBladeglow = new Color(48, 189, 191, 0);
            Color defaultCenter = new Color(150, 255, 255, 42);
            Color defaultCenterglow = new Color(16, 191, 191, 0);
            Color defaultRing = new Color(8, 225, 231, 177);
            Color defaultRingglow = new Color(1, 191, 191, 0);
            SetColor(defaultBlade, defaultBladeglow, defaultCenter, defaultCenterglow, defaultRing, defaultRingglow, defaultSymbol);
            beam.transform.localScale = new Vector3(1, 7, 1);
            beam.transform.localPosition = original;
            component = null;
            ringAbility?.SetActive(false);
            ringDefault?.SetActive(true);
            cycloneFX.SetActive(false);
        }
        public void SwapAbility(string ability)
        {
            switch (ability)
            {
                case "Default":
                    SetDefault();
                    break;
                case "MonadoBuster":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    beam.transform.localScale = new Vector3(1, 14, 1);
                    beam.transform.localPosition = new Vector3(original.x, original.y + 0.7f, original.z);
                    Color busterBlade = new Color(2, 2, 2, 1);
                    Color busterBladeglow = new Color(54, 186, 191, 0);
                    Color busterCenter = new Color(54, 186, 191, 42);
                    Color busterCenterglow = new Color(54, 186, 191, 0);
                    Color busterRing = new Color(54, 186, 191, 177);
                    Color busterRingglow = new Color(54, 186, 191, 0);
                    SetColor(busterBlade, busterBladeglow, busterCenter, busterCenterglow, busterRing, busterRingglow, busterSymbol);
                    component = "Buster";
                    break;
                case "MonadoEnchant":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color enchantBlade = new Color(0.992156863f, 0.0431372549f, 0.53333333333f, 1);
                    Color enchantBladeglow = new Color(119, 33, 191, 0);
                    Color enchantCenter = new Color(197, 109, 243, 42);
                    Color enchantCenterglow = new Color(119, 33, 191, 0);
                    Color enchantRing = new Color(197, 109, 243, 177);
                    Color enchantRingglow = new Color(119, 33, 191, 0);
                    SetColor(enchantBlade, enchantBladeglow, enchantCenter, enchantCenterglow, enchantRing, enchantRingglow, enchantSymbol);
                    component = "Enchant";
                    break;
                case "MonadoShield":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color shieldBlade = new Color(1, 0.164705882f, 0, 1);
                    Color shieldBladeglow = new Color(191, 91, 0, 0);
                    Color shieldCenter = new Color(255, 183, 0, 42);
                    Color shieldCenterglow = new Color(191, 137, 0, 0);
                    Color shieldRing = new Color(255, 183, 0, 177);
                    Color shieldRingglow = new Color(191, 91, 0, 0);
                    SetColor(shieldBlade, shieldBladeglow, shieldCenter, shieldCenterglow, shieldRing, shieldRingglow, shieldSymbol);
                    component = "Shield";
                    break;
                case "MonadoSpeed":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color speedBlade = new Color(0, 0.207843137f, 1, 1);
                    Color speedBladeglow = new Color(0, 79, 191, 0);
                    Color speedCenter = new Color(0, 172, 255, 42);
                    Color speedCenterglow = new Color(0, 79, 191, 0);
                    Color speedRing = new Color(0, 172, 255, 177);
                    Color speedRingglow = new Color(0, 79, 191, 0);
                    SetColor(speedBlade, speedBladeglow, speedCenter, speedCenterglow, speedRing, speedRingglow, speedSymbol);
                    component = "Speed";
                    break;
                case "MonadoPurge":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color purgeBlade = new Color(0.0431372549f, 1, 0.2f, 1);
                    Color purgeBladeglow = new Color(39, 191, 80, 0);
                    Color purgeCenter = new Color(40, 195, 114, 42);
                    Color purgeCenterglow = new Color(39, 191, 112, 0);
                    Color purgeRing = new Color(40, 195, 114, 177);
                    Color purgeRingglow = new Color(39, 191, 112, 0);
                    SetColor(purgeBlade, purgeBladeglow, purgeCenter, purgeCenterglow, purgeRing, purgeRingglow, purgeSymbol);
                    component = "Purge";
                    break;
                case "MonadoCyclone":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color cycloneBlade = new Color(1, 1, 1, 1);
                    Color cycloneBladeglow = new Color(191, 191, 191, 0);
                    Color cycloneCenter = new Color(255, 255, 255, 42);
                    Color cycloneCenterglow = new Color(191, 191, 191, 0);
                    Color cycloneRing = new Color(255, 255, 255, 177);
                    Color cycloneRingglow = new Color(191, 191, 191, 0);
                    SetColor(cycloneBlade, cycloneBladeglow, cycloneCenter, cycloneCenterglow, cycloneRing, cycloneRingglow, cycloneSymbol);
                    cycloneFX.SetActive(true);
                    component = "Cyclone";
                    break;
                case "MonadoArmor":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color armorBlade = new Color(1, 0.164705882f, 0, 1);
                    Color armorBladeglow = new Color(191, 91, 0, 0);
                    Color armorCenter = new Color(255, 183, 0, 42);
                    Color armorCenterglow = new Color(191, 137, 0, 0);
                    Color armorRing = new Color(255, 183, 0, 177);
                    Color armorRingglow = new Color(191, 91, 0, 0);
                    SetColor(armorBlade, armorBladeglow, armorCenter, armorCenterglow, armorRing, armorRingglow, armorSymbol);
                    component = "Armor";
                    break;
                case "MonadoEater":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color eaterBlade = new Color(0.0470588235f, 0, 1, 1);
                    Color eaterBladeglow = new Color(61, 18, 191, 0);
                    Color eaterCenter = new Color(47, 20, 82, 42);
                    Color eaterCenterglow = new Color(40, 11, 124, 0);
                    Color eaterRing = new Color(47, 20, 82, 177);
                    Color eaterRingglow = new Color(61, 18, 191, 0);
                    SetColor(eaterBlade, eaterBladeglow, eaterCenter, eaterCenterglow, eaterRing, eaterRingglow, eaterSymbol);
                    component = "Eater";
                    break;
            }
        }
        public void FixedUpdate()
        {
            if (item.IsHanded() && !wasGrabbed && item.mainHandler?.creature?.player != null)
            {
                wasGrabbed = true;
                AddMonadoSpells(Player.local.creature.container);
            }
            if (item.IsHanded() && triggerPressed && (item.physicBody.velocity - item.lastHandler.creature.currentLocomotion.rb.velocity).sqrMagnitude >= 225f && xenobladeRPG == null)
            {
                switch (component)
                {
                    case "Enchant":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId == item.lastHandler.creature.factionId && creature != item.lastHandler.creature && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<EnchantAura>());
                                creature.gameObject.AddComponent<EnchantAura>().Setup(EnchantAuraTime);
                            }
                        }
                        ActivateAura("MonadoEnchant");
                        SwapAbility("Default");
                        break;
                    case "Shield":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId == item.lastHandler.creature.factionId && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<ShieldAura>());
                                creature.gameObject.AddComponent<ShieldAura>().Setup(ShieldAuraTime, ShieldDamageAbsorb);
                            }
                        }
                        ActivateAura("MonadoShield");
                        SwapAbility("Default");
                        break;
                    case "Speed":
                        Destroy(item.lastHandler.creature.gameObject.GetComponent<SpeedAura>());
                        item.lastHandler.creature.gameObject.AddComponent<SpeedAura>().Setup(SpeedAuraTime, SpeedMultiplier);
                        ActivateAura("MonadoSpeed");
                        SwapAbility("Default");
                        break;
                    case "Cyclone":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId != item.lastHandler.creature.factionId && creature.isActiveAndEnabled && !creature.isKilled &&
                                Vector3.Distance(item.lastHandler.creature.transform.position, creature.transform.position) <= 25)
                            {
                                Destroy(creature.gameObject.GetComponent<CycloneAura>());
                                creature.gameObject.AddComponent<CycloneAura>().Setup(CycloneAuraTime);
                            }
                        }
                        ActivateAura("MonadoCyclone");
                        SwapAbility("Default");
                        break;
                    case "Armor":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId == item.lastHandler.creature.factionId && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<ArmorAura>());
                                creature.gameObject.AddComponent<ArmorAura>().Setup(ArmorAuraTime, ArmorDamageReductionMult);
                            }
                        }
                        ActivateAura("MonadoArmor");
                        SwapAbility("Default");
                        break;
                    default:
                        break;
                }
            }
            if (item.IsHanded() && triggerPressed && item.physicBody.GetPointVelocity(item.flyDirRef.position).magnitude - item.physicBody.GetPointVelocity(item.holderPoint.position).magnitude >= 10 && xenobladeRPG == null)
            {
                if (component == "Purge" || component == "Eater")
                {
                    foreach (Creature creature in Creature.allActive)
                    {
                        if (creature != null && creature != item.lastHandler.creature && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && creature.faction != item.lastHandler.creature.faction &&
                            Vector3.Dot(item.lastHandler.creature.centerEyes.forward, (creature.transform.position - item.lastHandler.creature.transform.position).normalized) >= 0.75f &&
                            Vector3.Distance(item.lastHandler.creature.transform.position, creature.transform.position) <= 25)
                        {
                            CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Energy, 20))
                            {
                                targetCollider = creature.ragdoll.rootPart.colliderGroup.colliders[0],
                                targetColliderGroup = creature.ragdoll.rootPart.colliderGroup,
                                sourceColliderGroup = item.colliderGroups[0],
                                sourceCollider = item.colliderGroups[0].colliders[0],
                                impactVelocity = item.physicBody.velocity,
                                contactPoint = creature.ragdoll.rootPart.transform.position,
                                contactNormal = -item.physicBody.velocity
                            };
                            instance.damageStruct.penetration = DamageStruct.Penetration.None;
                            instance.damageStruct.hitRagdollPart = creature.ragdoll.rootPart;
                            creature.Damage(instance);
                            if (component == "Purge")
                            {
                                Destroy(creature.gameObject.GetComponent<PurgeAura>());
                                creature.gameObject.AddComponent<PurgeAura>().Setup(PurgeAuraTime);
                            }
                            else if (component == "Eater")
                            {
                                creature.TryPush(Creature.PushType.Hit, (creature.transform.position - item.lastHandler.creature.transform.position).normalized, 1, instance.damageStruct.hitRagdollPart.type);
                                Destroy(creature.gameObject.GetComponent<EaterAura>());
                                creature.gameObject.AddComponent<EaterAura>().Setup(EaterAuraTime, EaterDamageOverTime);
                            }
                        }
                    }
                    if (component == "Purge")
                        ActivateAura("MonadoPurge");
                    else if (component == "Eater")
                        ActivateAura("MonadoEater");
                    EffectInstance effectInstance = Catalog.GetData<EffectData>("MonadoWave").Spawn(slash.transform.position, item.lastHandler.creature.centerEyes.rotation);
                    effectInstance.SetIntensity(1.0f);
                    effectInstance.Play();
                    SwapAbility("Default");
                }
            }
        }
        private void SetColor(Color blade, Color bladeEmission, Color centerColor, Color centerEmission, Color ring, Color ringEmission, GameObject symbol)
        {
            beam.GetComponent<VisualEffect>().SetVector4("BeamColor", new Vector4(blade.r * 2, blade.g * 2, blade.b * 2, blade.a));
            beamCross.GetComponent<VisualEffect>().SetVector4("BeamColor", new Vector4(blade.r * 2, blade.g * 2, blade.b * 2, blade.a));
            center.transform.Find("Area Light 0").GetComponent<Light>().color = bladeEmission * 0.01f;
            center.transform.Find("Area Light 1").GetComponent<Light>().color = bladeEmission * 0.01f;
            center.GetComponent<MeshRenderer>().material.SetColor("_Color", centerColor);
            center.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", centerEmission * 0.005f);
            ringAbility?.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", ring);
            ringAbility?.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", ringEmission * 0.03f);
            defaultSymbol.SetActive(false);
            busterSymbol.SetActive(false);
            enchantSymbol.SetActive(false);
            shieldSymbol.SetActive(false);
            speedSymbol.SetActive(false);
            purgeSymbol.SetActive(false);
            cycloneSymbol.SetActive(false);
            armorSymbol.SetActive(false);
            eaterSymbol.SetActive(false);
            symbol.SetActive(true);
        }
        public void ActivateAura(string effectId)
        {
            EffectInstance effectInstance = Catalog.GetData<EffectData>(effectId).Spawn(item.mainHandler.creature.transform.position, Quaternion.LookRotation(item.mainHandler.creature.transform.up), item.mainHandler.creature.transform, null, false);
            effectInstance.SetIntensity(0.1f);
            effectInstance.Play();
            if (Voices)
            {
                if (voice.isPlaying) voice.Stop();
                voice = Catalog.GetData<EffectData>(effectId + voiceText).Spawn(item.mainHandler.creature.transform.position, Quaternion.LookRotation(item.mainHandler.creature.transform.up), item.mainHandler.creature.transform, null, false);
                voice.SetIntensity(0.1f);
                voice.Play();
            }
        }
    }
    public class EnchantAura : MonoBehaviour
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
            instance = Catalog.GetData<EffectData>("MonadoEnchantAura").Spawn(creature.transform, null, false);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
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
    }
    public class ShieldAura : MonoBehaviour
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
            creature.OnDamageEvent += Creature_OnDamageEvent;
            timer = Time.time;
            instance = Catalog.GetData<EffectData>("MonadoShieldAura").Spawn(creature.transform, null, false);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
        }

        private void Creature_OnDamageEvent(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (collisionInstance?.damageStruct != null && eventTime == EventTime.OnStart && !collisionInstance.ignoreDamage)
            {
                absorb -= collisionInstance.damageStruct.damage;
                collisionInstance.damageStruct.damage = 0;
            }
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
            creature.OnDamageEvent -= Creature_OnDamageEvent;
        }
    }
    public class SpeedAura : MonoBehaviour
    {
        Creature creature;
        float timer;
        EffectInstance instance;
        float auraTime;
        float mult;
        public void Setup(float time, float multiplier)
        {
            auraTime = time;
            mult = multiplier;
        }
        public void Start()
        {
            creature = GetComponent<Creature>();
            timer = Time.time;
            creature.currentLocomotion.SetSpeedModifier(this, mult, mult, mult, mult, mult);
            instance = Catalog.GetData<EffectData>("MonadoSpeedAura").Spawn(creature.transform, null, false);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
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
            creature.currentLocomotion.RemoveSpeedModifier(this);
            instance.Stop();
        }
    }
    public class PurgeAura : MonoBehaviour
    {
        Creature creature;
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
            timer = Time.time;
            creature.TryElectrocute(1, 3, true, false, Catalog.GetData<EffectData>("ImbueLightningRagdoll", true));
            instance = Catalog.GetData<EffectData>("MonadoPurgeAura").Spawn(creature.transform, null, false);
            creature.ragdoll.AddPhysicToggleModifier(this);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
        }
        public void FixedUpdate()
        {
            if (Time.time - timer >= auraTime || creature.isKilled)
            {
                instance.Stop();
                creature.ragdoll.RemovePhysicToggleModifier(this);
                Destroy(this);
            }
            else
            {
                creature.mana.currentMana = 0f;
                Destroy(creature.gameObject.GetComponent<EnchantAura>());
                Destroy(creature.gameObject.GetComponent<ShieldAura>());
                Destroy(creature.gameObject.GetComponent<SpeedAura>());
                Destroy(creature.gameObject.GetComponent<ArmorAura>());
            }
        }
    }
    public class CycloneAura : MonoBehaviour
    {
        Creature creature;
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
            timer = Time.time;
            creature.ragdoll.SetState(Ragdoll.State.Destabilized);
            instance = Catalog.GetData<EffectData>("MonadoCycloneAura").Spawn(creature.transform, null, false);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
            creature.brain.AddNoStandUpModifier(this);
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
            creature.brain.RemoveNoStandUpModifier(this);
        }
    }
    public class ArmorAura : MonoBehaviour
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
            creature.OnDamageEvent += Creature_OnDamageEvent;
            timer = Time.time;
            instance = Catalog.GetData<EffectData>("MonadoArmorAura").Spawn(creature.transform, null, false);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
        }

        private void Creature_OnDamageEvent(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (collisionInstance?.damageStruct != null && eventTime == EventTime.OnStart && !collisionInstance.ignoreDamage)
            {
                collisionInstance.damageStruct.damage *= 1 - mult;
            }
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
        }
    }
    public class EaterAura : MonoBehaviour
    {
        Creature creature;
        float timer;
        float cooldown;
        EffectInstance instance;
        float auraTime;
        float dot;
        public void Setup(float time, float damage)
        {
            auraTime = time;
            dot = damage;
        }
        public void Start()
        {
            creature = GetComponent<Creature>();
            timer = Time.time;
            cooldown = Time.time;
            instance = Catalog.GetData<EffectData>("MonadoEaterAura").Spawn(creature.transform, null, false);
            instance.SetRenderer(creature.GetRendererForVFX(), false);
            instance.SetIntensity(1f);
            instance.Play();
            Destroy(creature.gameObject.GetComponent<EnchantAura>());
            Destroy(creature.gameObject.GetComponent<ShieldAura>());
            Destroy(creature.gameObject.GetComponent<SpeedAura>());
            Destroy(creature.gameObject.GetComponent<ArmorAura>());
            creature.ragdoll.AddPhysicToggleModifier(this);
        }
        public void FixedUpdate()
        {
            if (Time.time - timer >= auraTime || creature.isKilled)
            {
                instance.Stop();
                creature.ragdoll.RemovePhysicToggleModifier(this);
                Destroy(this);
            }
            else if (Time.time - cooldown >= 1.5f)
            {
                CollisionInstance collision = new CollisionInstance(new DamageStruct(DamageType.Unknown, dot));
                collision.damageStruct.hitRagdollPart = creature.ragdoll.rootPart;
                cooldown = Time.time;
                creature.Damage(collision);
            }
        }
    }
}
