namespace Essentials.Options
{
    //public class CustomKeyValueOption : CustomStringOption
    //{
    //    protected internal CustomKeyValueOption(string id, string name, bool saveValue, string[] values) : base(id, name, saveValue, values)
    //    {
    //        GameSettingCallback = new Action<OptionBehaviour>(GameOptionCreated);
    //    }

    //    private void GameOptionCreated(OptionBehaviour o)
    //    {
    //        if (!(o is KeyValueOption kv)) return;

    //        kv.OnValueChanged = new Action<OptionBehaviour>((option) => { });

    //        kv.name = kv.gameObject.name = ID;

    //        kv.TitleText.Text = Name;
    //        if(kv.Values!=null)
    //        {
    //            kv.Values.Clear();
    //            for (int i = 0; i < Values.Length; i++) kv.Values.Add(new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>(Values[i], i));
    //        }
    //        /*kv.Values = new List<Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
    //        for (int i = 0; i < Values.Length; i++) kv.Values.Add(new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>(Values[i], i));*/
    //        kv.Selected = kv.oldValue = GetValue();
    //        kv.ValueText.Text = GetFormattedValue();

    //        GameSetting = kv;
    //    }
    //}

    /*public partial class CustomOption
    {
        public static CustomKeyValueOption AddKeyValue(string id, string name, string[] values, Action<int> callback)
        {
            CustomKeyValueOption option = new CustomKeyValueOption(id, name, values);
            
            void KeyValueCallback(object newVal, bool report)
            {
                int newValue = (int)newVal;
                
                //int oldValue = (int)option.Value;
                option.Value = newValue;
                
                if (option.GameSetting is KeyValueOption kv)
                {
                    if (report && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
                    {
                        Rpc.Instance.Send(new Rpc.Data(option));

                        option.ConfigEntry.Value = newValue;
                    }

                    //kv.oldValue = oldValue;
                    kv.Selected = kv.oldValue = newValue;
                    kv.ValueText.Text = option.GetFormattedValue();
                }
                
                try
                {
                    callback?.Invoke(newValue);
                }
                catch (Exception e)
                {
                    EssentialsPlugin.Logger.LogInfo($"KeyValueCallback \"{name}\" failed: {e}");
                }
            }
            
            option.Callback = new Action<object, bool>(KeyValueCallback);
            
            if (option.Value != option.DefaultValue) option.Callback.Invoke(option.Value, false);
            
            return option;
        }

        public static CustomKeyValueOption AddKeyValue(string id, string name, params string[] values)
        {
            return AddKeyValue(id, name, values, null);
        }

        public static CustomKeyValueOption AddKeyValue(string name, params string[] values)
        {
            return AddKeyValue(name, name, values);
        }
    }*/
}