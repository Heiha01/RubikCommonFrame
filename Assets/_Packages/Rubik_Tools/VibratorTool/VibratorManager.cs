using System;
using UnityEngine;
using UtilsModule;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rubik_Tools.Vibrator{
    public enum HapticTypes
    {
        Selection,
        LightImpact,
        MediumImpact,
        HeavyImpact,
        Success,
        Warning,
        Failure
    }
    public class VibratorManager : ManagerBase<VibratorManager>
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _PlayTaptic(int type);
        [DllImport("__Internal")]
        private static extern void _PlayTaptic6s(int type);
#endif
        private static Action<bool> _onSwitch;

        private static bool _vibratorEnable;
        private static bool _isGotEnableState;
        public override void Init()
        {

        }

        /// <summary>
        /// Vibrator是否开启
        /// </summary>
        private static bool VibratorEnable
        {
            get
            {
                if (!_isGotEnableState)
                {
                    _vibratorEnable = PlayerPrefs.GetInt("VibratorEnable", 1) == 1;
                    _isGotEnableState = true;
                }
                return _vibratorEnable;
            }
            set
            {
                _vibratorEnable = value;
                PlayerPrefs.SetInt("VibratorEnable", value ? 1 : 0);
                _onSwitch?.Invoke(value);
            }
        }

        /// <summary>
        /// 调用震动
        /// </summary>
        /// <param name="level"></param>
        public static void Trigger(HapticTypes level)
        {
            try
            {

                if (VibratorEnable)
                {
                    UpdateTrigger(level);


#if PUFFER_DEBUG
                Debug.Log("[VibratorManager]:Trigger"+level);
#endif
                }
                else
                {
#if PUFFER_DEBUG
                Debug.Log("[VibratorManager]:VibratorEnable==False");
#endif
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error:" + e.Message);
            }

        }

        private static float _lastTriggerTime = 0;
        /// <summary>
        /// 在Update中调用，获得一个连续固定间隔的振动效果
        /// </summary>
        /// <param name="level">振动等级,详见Tigger方法注释</param>
        /// <param name="interval">振动间隔</param>
        public static void UpdateTrigger(HapticTypes level = HapticTypes.Selection, float interval = 0.05f)
        {
            if (Time.time - _lastTriggerTime < interval)
                return;
            if (!VibratorEnable)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidTaptic.Haptic((HapticTypes)level);
#endif
#if UNITY_IOS && !UNITY_EDITOR
               if (IsTapticEngine())
            {
               _PlayTaptic((int)level);
            }
            else
            {  
                _PlayTaptic6s((int)level);
            }
#endif


            _lastTriggerTime = Time.time;
        }




        /// <summary>
        /// 设备是否支持TapticEngine
        /// </summary>
        /// <returns><c>true</c>, if taptic engine was ised, <c>false</c> otherwise.</returns>
        private static bool IsTapticEngine()
        {
            try
            {
                if (IsiPadOriPod())
                    return false;
                var s = SystemInfo.deviceModel;
                int iPhoneId;
                if (s[7].Equals(','))
                    iPhoneId = int.Parse(s[6].ToString());
                else
                    iPhoneId = int.Parse(s[6] + "" + s[7]);
                return iPhoneId > 8;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }


            //return SystemInfo.deviceModel == "iPhone8,1" || SystemInfo.deviceModel == "iPhone8,2";
        }

        /// <summary>
        /// 判断设备是否为ipad或iPod,在ipad上应该隐藏震动按钮
        /// </summary>
        /// <returns><c>true</c>, if pad was isied, <c>false</c> otherwise.</returns>
        public static bool IsiPadOriPod()
        {
            return SystemInfo.deviceModel.Contains("Pad") || SystemInfo.deviceModel.Contains("Pod");
        }

        public static void Switch()
        {
            VibratorEnable = !VibratorEnable;
        }

        /// <summary>
        /// 绑定事件
        /// </summary>
        /// <param name="actionEvent"></param>
        public static void BindEvent(System.Action<bool> actionEvent)
        {
            _onSwitch += actionEvent;
            actionEvent?.Invoke(VibratorEnable);
        }

    }

}