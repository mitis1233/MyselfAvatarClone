using ExitGames.Client.Photon;
using MelonLoader;
using System.Reflection;
using UnityEngine;
using VRC.Core;
using VRC.DataModel;
using System.Linq;
using System;
using System.Collections;
using UnhollowerRuntimeLib.XrefScans;
using VRC;

[assembly: MelonGame("VRChat")]
[assembly: MelonInfo(typeof(MyselfAvatarClone.MyselfAvatarClone), "MyselfAvatarClone", "1.0.2")]

namespace MyselfAvatarClone
{
    public class MyselfAvatarClone : MelonMod
    {
        private static bool _state;
        private static Il2CppSystem.Object AvatarDictCache { get; set; }
        private static MethodInfo _loadAvatarMethod;
        GameObject quickMenu;


        private static void Log(string message)
        {
            VRCUiManager.prop_VRCUiManager_0.field_Private_List_1_String_0.Add(message);
        }

        private static void Detour(ref EventData __0)
        {
            if (_state
                && __0.Code == 253
                && AvatarDictCache != null
                && __0.Sender == VRC.Player.prop_Player_0.field_Private_VRCPlayerApi_0.playerId
            ) __0.Parameters[251].Cast<Il2CppSystem.Collections.Hashtable>()["avatarDict"] = AvatarDictCache;
        }

        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(QMInitializer());

            HarmonyInstance.Patch(
                typeof(VRCNetworkingClient).GetMethod(nameof(VRCNetworkingClient.OnEvent)),
                typeof(MyselfAvatarClone).GetMethod(nameof(Detour), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod()
            );
            _loadAvatarMethod =
                typeof(VRCPlayer).GetMethods()
                .First(mi =>
                    mi.Name.StartsWith("Method_Private_Void_Boolean_")
                    && mi.Name.Length < 31
                    && mi.GetParameters().Any(pi => pi.IsOptional)
                    && XrefScanner.UsedBy(mi) // Scan each method
                        .Any(instance => instance.Type == XrefType.Method
                            && instance.TryResolve() != null
                            && instance.TryResolve().Name == "ReloadAvatarNetworkedRPC"));
        }

        private IEnumerator QMInitializer()
        {
            while ((quickMenu = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)")) == null)
                yield return null;
            SelMenu();
        }
        
        private void SelMenu()
        {
            Transform ButtonTP = quickMenu.transform.Find("Container/Window/QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UserActions");
            Utils.CreateDefaultButton("複製Avatar", new Vector3(0, -25, 0), "更換Avatar只對自己顯示", Color.white, ButtonTP,
                new Action(() => {
                    if (UserSelectionManager.field_Private_Static_UserSelectionManager_0.field_Private_APIUser_1 == null)
                    {
                        return;
                    }

                    string target = UserSelectionManager.field_Private_Static_UserSelectionManager_0.field_Private_APIUser_1.id;
                    AvatarDictCache = PlayerManager.prop_PlayerManager_0
                        .field_Private_List_1_Player_0
                        .ToArray()
                        .FirstOrDefault(a => a.field_Private_APIUser_0.id == target)
                        ?.prop_Player_1.field_Private_Hashtable_0["avatarDict"];
                    _loadAvatarMethod.Invoke(VRCPlayer.field_Internal_Static_VRCPlayer_0, new object[] { true });
                }));
            Utils.CreateDefaultButton("開關Avatar顯示", new Vector3(0, -25, 0), "開關複製Avatar功能 只對自己顯示", Color.white, ButtonTP,
                new Action(() => {
                    _state ^= true;
                    Log(_state ? "開啟" : "關閉");
                }));
        }
    }
}
