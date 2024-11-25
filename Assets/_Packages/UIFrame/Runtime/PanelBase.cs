using System;
using DG.Tweening;
using UnityEngine;

namespace Tools.UI
{
    public class PanelBase : MonoBehaviour
    {
        [SerializeField] private float showDuration = 0.3f;
        /*[SerializeField] private bool isAutoShow;
        [SerializeField] private int orderInLayer = 1;*/
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;

        protected Action OnCloseComplete;
        private UITweenerInterface[] _uiTweeners;
        private bool _isShowing;
        private bool _isInit;

        public int OrderInLayer
        {
            get => PCanvas.sortingOrder;
            set => PCanvas.sortingOrder = value;
        }

        public bool IsShowing => _isShowing;

        protected CanvasGroup PCanvasGroup
        {
            get
            {
                if (!_canvasGroup)
                {
                    _canvasGroup = gameObject.GetComponent<CanvasGroup>();
                    if (!_canvasGroup) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    _canvasGroup.interactable = true;
                    if (_isShowing)
                    {
                        _canvasGroup.alpha = 1;
                        _canvasGroup.blocksRaycasts = true;
                    }
                    else
                    {
                        _canvasGroup.alpha = 0;
                        _canvasGroup.blocksRaycasts = false;
                    }
                }

                return _canvasGroup;
            }
        }
        
        protected Canvas PCanvas
        {
            get
            {
                if (!_canvas)
                {
                    _canvas = GetComponent<Canvas>();
                }

                return _canvas;
            }
        }

        protected UITweenerInterface[] UiTweeners
        {
            get
            {
                if (!_isInit)
                {
                    _isInit = true;
                    _uiTweeners = GetComponentsInChildren<UITweenerInterface>();
                }

                return _uiTweeners;
            }
        }

        public virtual void Init()
        {
            if (!PCanvas.SafeIsUnityNull())
            {
                PCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                PCanvas.worldCamera = GameConfigManager.Instance.UiCamera;
                PCanvas.enabled = false;
            }
        }

        public virtual void InvokeMethod(string methodName, params object[] args)
        {
        }

        public virtual void Open(params object[] args)
        {
            if (_isShowing) return;
            _isShowing = true;
            
            Open(showDuration);
        }

        public virtual void Close()
        {
            if (!_isShowing) return;
            Close(showDuration);
        }

        protected virtual void CloseComplete()
        {
            PCanvas.enabled = false;
            _isShowing = false;
            OnCloseComplete?.Invoke();
        }

        protected virtual void Open(float duration)
        {
            PCanvas.enabled = true;
            PCanvasGroup.blocksRaycasts = true;

            if (duration > 0.1f)
            {
                PCanvasGroup.DOFade(1, duration).SetEase(Ease.Linear);
            }

            if (UiTweeners != null)
            {
                foreach (var uiAnimationCtrl in UiTweeners)
                {
                    uiAnimationCtrl.ToOpen();
                }
            }
        }

        protected virtual void Close(float duration, Action onComplete = null)
        {
            OnCloseComplete += onComplete;

            if (duration <= 0)
            {
                PCanvasGroup.blocksRaycasts = false;
                if (UiTweeners != null)
                {
                    foreach (var uiAnimationCtrl in UiTweeners)
                    {
                        uiAnimationCtrl.ToClose();
                    }
                }
                CloseComplete();
            }
            else
            {
                PCanvasGroup.blocksRaycasts = false;
                PCanvasGroup.DOFade(0, duration).SetEase(Ease.Linear).OnComplete(CloseComplete);
                if (UiTweeners != null)
                {
                    foreach (var uiAnimationCtrl in UiTweeners)
                    {
                        uiAnimationCtrl.ToClose();
                    }
                }
            }
        }

        #region EditorMethod

        [ButtonGroup("Group", GroupID = "1")]
        [Button(ButtonSizes.Medium, Name = "Close")]
        private void OnEditorClose()
        {
            if (Application.isPlaying)
            {
                Close();
            }
            else
            {
                PCanvas.enabled = false;
            }
        }

        [ButtonGroup("Group", GroupID = "1")]
        [Button(ButtonSizes.Medium, Name = "Open")]
        private void OnEditorOpen()
        {
            if (Application.isPlaying)
            {
                Open();
            }
            else
            {
                PCanvas.enabled = true;
            }
        }

        #endregion
    }
}