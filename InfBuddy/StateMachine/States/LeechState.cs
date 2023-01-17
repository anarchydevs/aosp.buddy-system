using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class LeechState : IState
    {
        private static bool _missionsLoaded = false;

        private bool _init = false;
        private bool _asyncToggle = false;

        public static double _timeOut = Time.NormalTime;

        Item stackitem;
        int stackingslot = (int)EquipSlot.Cloth_RightFinger;

        public static Container stackBag = Inventory.Backpacks.FirstOrDefault(x => x.Name == "stack1");

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.OmniPandeGId || Playfield.ModelIdentity.Instance == Constants.ClanPandeGId)
                return new DiedState();

            if (_missionsLoaded && !Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                return new ExitMissionState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("LeechState::OnStateEnter");

            DynelManager.LocalPlayer.Position = Constants.LeechSpot;
            MovementController.Instance.SetMovement(MovementAction.Update);
            MovementController.Instance.SetMovement(MovementAction.JumpStart);
            MovementController.Instance.SetMovement(MovementAction.Update);
        }

        public void OnStateExit()
        {
            Chat.WriteLine("LeechState::OnStateExit");

            _missionsLoaded = false;
            _init = false;
            DynelManager.LocalPlayer.Position = new Vector3(160.4f, 2.6f, 103.0f);
        }

        public void Tick()
        {
            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;

            if (!_init && !_asyncToggle)
            {
                Task.Factory.StartNew(
                    async () =>
                    {
                        _asyncToggle = true;

                        await Task.Delay(3000);

                        Item bank = Inventory.Items.Where(c => c.Name == "Portable Bank Terminal").FirstOrDefault();
                        Item bag = Inventory.Items.Where(c => c.HighId == 296977).FirstOrDefault();

                        await Task.Delay(11000);


                        if (bank != null)
                            bank.Use();

                        if (bag != null)
                            bag.Use();

                        await Task.Delay(1000);

                        if (bank != null)
                            bank.Use();

                        await Task.Delay(2000);

                        stackitem = Inventory.Items.Where(c => c != null && c.Slot.Type == IdentityType.ArmorPage
                                        && c.Slot.Instance == (int)EquipSlot.Cloth_RightFinger).FirstOrDefault();

                        await Task.Delay(2000);

                        if (stackitem == null)
                            stackitem = stackBag.Items.FirstOrDefault();

                        await Task.Delay(2000);

                        stackitem?.MoveToContainer(stackBag);

                        await Task.Delay(1000);

                        _timeOut = Time.NormalTime;
                        _init = true;
                        _asyncToggle = false;
                    });
            }

            if (_init && Time.NormalTime - _timeOut < 35f)
            {
                for (int i = 1; i <= 20; i++)
                {
                    Identity stackBagId = stackBag.Identity;
                    Identity bank = new Identity();
                    bank.Type = IdentityType.BankByRef;
                    bank.Instance = (int)stackingslot;

                    EquipItem(stackBag, EquipSlot.Cloth_RightFinger);
                    StripItem(bank, stackBag);
                }
            }
        }

        private static void StripItem(Identity bank, Container stackBag)
        {
            Network.Send(new ClientContainerAddItem()
            {
                Target = stackBag.Identity,
                Source = bank
            });
        }

        private static void EquipItem(Container stackBag, EquipSlot slotToStack)
        {
            foreach (Item item in stackBag.Items)
            {
                item.Equip(slotToStack);
                break;
            }
        }
    }
}
