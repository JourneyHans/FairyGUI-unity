using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// UI手感调整管理器
/// </summary>
public static class UIFeelConfig {
    private class Data {
        public float pagingSensitive;
        public float pagingDuration;
        public float pageQuickDragThreshold;
        public float pageSlowMoveTolerance;
        public int vCustomScrollSensitive;
        public int hCustomScrollSensitive;
        public int scrollDampingRate;
        public int easeType;
    }

    private static Data _uiConfigData;

    public static float PagingSensitive {
        get => _uiConfigData.pagingSensitive;
        set => _uiConfigData.pagingSensitive = value;
    }

    public static float PagingDuration {
        get => _uiConfigData.pagingDuration;
        set => _uiConfigData.pagingDuration = value;
    }

    public static float PageQuickDragThreshold {
        get => _uiConfigData.pageQuickDragThreshold;
        set => _uiConfigData.pageQuickDragThreshold = value;
    }

    public static float PageSlowMoveTolerance {
        get => _uiConfigData.pageSlowMoveTolerance;
        set => _uiConfigData.pageSlowMoveTolerance = value;
    }

    public static int VCustomScrollSensitive {
        get => _uiConfigData.vCustomScrollSensitive;
        set => _uiConfigData.vCustomScrollSensitive = value;
    }

    public static int HCustomScrollSensitive {
        get => _uiConfigData.hCustomScrollSensitive;
        set => _uiConfigData.hCustomScrollSensitive = value;
    }

    public static int ScrollDampingRate {
        get => _uiConfigData.scrollDampingRate;
        set => _uiConfigData.scrollDampingRate = value;
    }

    public static int EaseType {
        get => _uiConfigData.easeType;
        set => _uiConfigData.easeType = value;
    }

    public static float EaseFunc(float time, float duration) {
        switch (_uiConfigData.easeType) {
            case 1:     // OutCubic
                return (time = time / duration - 1) * time * time + 1;
            case 2:     // OutQuart
                return -((time = time / duration - 1) * time * time * time - 1);
            case 3:     // OutQuint
                return ((time = time / duration - 1) * time * time * time * time + 1);
            case 4:     // OutExpo
                if (time == duration) return 1;
                return (-(float) Math.Pow(2, -10 * time / duration) + 1);
            case 5:     // OutCirc
                return (float) Math.Sqrt(1 - (time = time / duration - 1) * time);
        }
        // default is OutCubic
        return (time = time / duration - 1) * time * time + 1;
    }
    
    public static void Load() {
#if _DEV_INTERNAL_
        LoadByFile();
#else
        LoadFormally();
#endif
    }

#if _DEV_INTERNAL_
    private static void LoadByFile() {
        string configFilePath = Application.persistentDataPath + "/ui_feel_config.json";
        if (!File.Exists(configFilePath)) {
            LoadFormally();
            SaveConfigFile();
        }
        else {
            string jsonStr = File.ReadAllText(configFilePath);
            _uiConfigData = JsonConvert.DeserializeObject<Data>(jsonStr);
            // 检测完整性
            if (_uiConfigData.pagingSensitive == 0f) {
                _uiConfigData.pagingSensitive = 0.06f;
            }

            if (_uiConfigData.pagingDuration == 0f) {
                _uiConfigData.pagingDuration = 0.7f;
            }

            if (_uiConfigData.pageQuickDragThreshold == 0f) {
                _uiConfigData.pageQuickDragThreshold = 0.2f;
            }

            if (_uiConfigData.pageSlowMoveTolerance == 0f) {
                _uiConfigData.pageSlowMoveTolerance = 0.5f;
            }

            if (_uiConfigData.vCustomScrollSensitive == 0) {
                _uiConfigData.vCustomScrollSensitive = 15;
            }

            if (_uiConfigData.hCustomScrollSensitive == 0) {
                _uiConfigData.hCustomScrollSensitive = 5;
            }

            if (_uiConfigData.scrollDampingRate == 0) {
                _uiConfigData.scrollDampingRate = 60;
            }

            if (_uiConfigData.easeType == 0) {
                _uiConfigData.easeType = 3;
            }
        }
    }

    public static void SaveConfigFile() {
        string configFilePath = Application.persistentDataPath + "/ui_feel_config.json";
        File.Delete(configFilePath);
        string json = JsonConvert.SerializeObject(_uiConfigData);
        File.WriteAllText(configFilePath, json);
    }
#endif

    // 正式配置直接读
    private static void LoadFormally()
    {
        _uiConfigData = new Data {
            pagingSensitive = 0.06f,
            pagingDuration = 0.7f,
            pageQuickDragThreshold = 0.2f,
            pageSlowMoveTolerance = 0.5f,
            vCustomScrollSensitive = 15,
            hCustomScrollSensitive = 5,
            scrollDampingRate = 60,
            easeType = 3
        };
    }
}
