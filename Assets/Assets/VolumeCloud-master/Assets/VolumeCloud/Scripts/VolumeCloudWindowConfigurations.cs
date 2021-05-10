﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yangrc.VolumeCloud {

    public class VolumeCloudWindowConfigurations : ScriptableObject {
        public CurlNoiseGenerator testGenerator;

        public First3DTexGenerator first3DTexGenerator;
        public string first3DTexSaveName = "First3DTex";
        public Second3DTexGenerator second3DTexGenerator;
        public string second3DTexSaveName = "Second3DTex";
    }
}
