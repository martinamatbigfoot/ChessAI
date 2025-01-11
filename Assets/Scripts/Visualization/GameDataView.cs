using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RookWorks.Visualization
{
    public class GameDataView : MonoBehaviour
    {
        [SerializeField]
        private Button _button;
        [SerializeField]
        private TextMeshProUGUI _info;

        private Guid _guid;
        private Action<Guid> _callback;
        
        public void Initialize(Dictionary<string, string> metadata, Action<Guid> callback)
        {
            _guid = Guid.Parse(metadata["GUID"]);
            _callback = callback;
            var utcDate = metadata["UTCDate"];
            var utcTime = metadata["UTCTime"];
            var whiteName = metadata["White"];
            var blackName = metadata["Black"];
            var result = metadata["Result"];
            _info.text = $"{utcDate} {utcTime} \n {whiteName} - {blackName} ({result})";
            _button.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            _callback?.Invoke(_guid);
        }
    }
}
