using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{

    public class HeroSelection : MonoBehaviour
    {
        private GameObject _heroListContent;
        private ToggleGroup _heroListToggleGroup;
        private Button _enterButton;
        // Start is called before the first frame update
        void Start()
        {
            _heroListContent = transform.Find("HeroList/Viewport/Content").gameObject;
            _heroListToggleGroup = _heroListContent.GetComponent<ToggleGroup>();
            _enterButton = transform.Find("EnterButton").GetComponent<Button>();
            //初始化英雄
            int i = 0;
            foreach (var heroInfo in UserData.AllHero)
            {
                var heroItem = GameObject.Instantiate(Resources.Load<GameObject>("UI/HeroSelection/HeroItem"));
                heroItem.transform.SetParent(_heroListContent.transform, false);
                var textName = heroItem.transform.Find("Label").GetComponent<Text>();
                var toggle = heroItem.GetComponent<Toggle>();

                textName.text = heroInfo.name;
                toggle.group = _heroListToggleGroup;
                //用闭包实现角色索引和Toggle绑定
                var index = i;
                i++;
                toggle.onValueChanged.AddListener((isOn) => { onToggleValueChanged(index, isOn); });
            }
        }

        private void onToggleValueChanged(int heroIndex, bool isOn)
        {
            Debug.Log(String.Format("{0}\t{1}",heroIndex,isOn));
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
