using System;
using DG.Tweening;
using Rubik_Tools.Vibrator;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UtilsModule;

namespace Rubik_Tools
{
    public class ButtonExtend : Button
    {
        public bool isCloseTween = false;
        private Tweener _clickTweener;

        private Action _onPointerDown;
        private Action _onPointerUp;

        public Action OnPointerDownAction
        {
            get => _onPointerDown;
            set => _onPointerDown = value;
        }

        public Action OnPointerUpAction
        {
            get => _onPointerUp;
            set => _onPointerUp = value;
        }

        protected override void Awake()
        {
            if (!isCloseTween)
            {
                _clickTweener = transform.DOScale(
                        transform.localScale - new Vector3(0.1f, 0.1f, 0.1f),
                        0.1f)
                    .SetAutoKill(false)
                    .SetEase(Ease.Linear)
                    .Pause();
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;
 
            if (!isCloseTween)
            {
                _clickTweener.PlayForward();
            }
            _onPointerDown?.Invoke();
            VibratorManager.Trigger(HapticTypes.Selection);
            AudioManager.Instance.PlaySFX(StrConstantsContainer.AudioClipPath.Click);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!isCloseTween)
            {
                _clickTweener.PlayBackwards();
            }
            _onPointerUp?.Invoke();

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _clickTweener.Kill();
        }
    }
}