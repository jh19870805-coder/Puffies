// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:BF/Effect/A/AParticleFireClip01,iptp:0,cusa:True,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:0,trmd:0,grmd:0,uamb:False,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:3,stfa:3,stfz:3,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:7142,x:34786,y:32648,varname:node_7142,prsc:2|emission-9926-OUT,custl-6219-OUT,alpha-9640-OUT,clip-6254-OUT;n:type:ShaderForge.SFN_Tex2d,id:8312,x:31172,y:31929,varname:_MainTexaa,prsc:2,ntxv:0,isnm:False|UVIN-89-OUT,TEX-9152-TEX;n:type:ShaderForge.SFN_Color,id:4901,x:32776,y:30921,ptovrint:False,ptlb:Light,ptin:_Light,varname:_Light,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:5763,x:30926,y:32524,ptovrint:False,ptlb:[MaskTex],ptin:_MaskTex,varname:_MaskTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-8218-OUT;n:type:ShaderForge.SFN_Slider,id:432,x:31089,y:33145,ptovrint:False,ptlb:[MaskExp],ptin:_MaskExp,varname:_MaskExp,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:0.6939077,max:0;n:type:ShaderForge.SFN_Color,id:7834,x:32278,y:31139,ptovrint:False,ptlb:Gray,ptin:_Gray,varname:_Gray,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.490566,c2:0.490566,c3:0.490566,c4:1;n:type:ShaderForge.SFN_Lerp,id:7608,x:32628,y:31118,varname:node_7608,prsc:2|A-1820-RGB,B-7834-RGB,T-49-OUT;n:type:ShaderForge.SFN_TexCoord,id:108,x:27315,y:33981,varname:node_108,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Append,id:8489,x:28412,y:33745,varname:node_8489,prsc:2|A-6919-OUT,B-739-OUT;n:type:ShaderForge.SFN_Time,id:6037,x:27302,y:33573,varname:node_6037,prsc:2;n:type:ShaderForge.SFN_Add,id:6919,x:28102,y:33657,varname:node_6919,prsc:2|A-7274-OUT,B-108-U,C-9574-A;n:type:ShaderForge.SFN_Add,id:739,x:28063,y:33906,varname:node_739,prsc:2|A-1893-OUT,B-108-V;n:type:ShaderForge.SFN_Vector4Property,id:5000,x:27315,y:33764,ptovrint:False,ptlb:NoiseSpeed,ptin:_NoiseSpeed,varname:_NoiseSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Multiply,id:7274,x:27762,y:33535,varname:node_7274,prsc:2|A-6037-T,B-5000-X;n:type:ShaderForge.SFN_Multiply,id:1893,x:27762,y:33697,varname:node_1893,prsc:2|A-6037-T,B-5000-Y;n:type:ShaderForge.SFN_Rotator,id:82,x:28711,y:33755,varname:node_82,prsc:2|UVIN-8489-OUT,SPD-5000-Z;n:type:ShaderForge.SFN_Multiply,id:699,x:30043,y:33407,varname:node_699,prsc:2|A-1248-OUT,B-4705-OUT;n:type:ShaderForge.SFN_Slider,id:1248,x:29643,y:33173,ptovrint:False,ptlb:Noise,ptin:_Noise,varname:_Noise,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Add,id:89,x:30606,y:31931,varname:node_89,prsc:2|A-4198-OUT,B-699-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:8595,x:28880,y:33991,ptovrint:False,ptlb:NoiseTex,ptin:_NoiseTex,varname:_NoiseTex,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9497,x:29111,y:33779,varname:node_9497,prsc:2,ntxv:0,isnm:False|UVIN-7166-OUT,TEX-8595-TEX;n:type:ShaderForge.SFN_RemapRange,id:8692,x:31498,y:33166,varname:node_8692,prsc:2,frmn:1,frmx:0,tomn:0,tomx:1|IN-432-OUT;n:type:ShaderForge.SFN_Append,id:968,x:29185,y:29671,varname:node_968,prsc:2|A-9294-OUT,B-9706-OUT;n:type:ShaderForge.SFN_Time,id:5254,x:28174,y:29497,varname:node_5254,prsc:2;n:type:ShaderForge.SFN_Add,id:9294,x:28782,y:29663,varname:node_9294,prsc:2|A-8622-OUT,B-241-R,C-9574-R;n:type:ShaderForge.SFN_Add,id:9706,x:28782,y:29929,varname:node_9706,prsc:2|A-9474-OUT,B-241-G;n:type:ShaderForge.SFN_Vector4Property,id:2802,x:28174,y:29755,ptovrint:False,ptlb:[MainTexMove],ptin:_MainTexMove,varname:_MainTexMove,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Multiply,id:8622,x:28417,y:29515,varname:node_8622,prsc:2|A-5254-T,B-2802-X;n:type:ShaderForge.SFN_Multiply,id:9474,x:28417,y:29702,varname:node_9474,prsc:2|A-5254-T,B-2802-Y;n:type:ShaderForge.SFN_Rotator,id:7522,x:29717,y:29740,varname:node_7522,prsc:2|UVIN-968-OUT,ANG-2187-OUT;n:type:ShaderForge.SFN_Pi,id:3335,x:29292,y:29985,varname:node_3335,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2187,x:29488,y:29841,varname:node_2187,prsc:2|A-1650-OUT,B-3335-OUT;n:type:ShaderForge.SFN_RemapRange,id:1650,x:29185,y:29839,varname:node_1650,prsc:2,frmn:0,frmx:360,tomn:0,tomx:2|IN-2802-Z;n:type:ShaderForge.SFN_Desaturate,id:8085,x:31179,y:32494,varname:node_8085,prsc:2|COL-5763-RGB;n:type:ShaderForge.SFN_Multiply,id:8029,x:33025,y:33188,varname:node_8029,prsc:2|A-3505-OUT,B-92-OUT;n:type:ShaderForge.SFN_Slider,id:92,x:32630,y:33495,ptovrint:False,ptlb:[AlphaScale],ptin:_AlphaScale,varname:_AlphaScale,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:10;n:type:ShaderForge.SFN_TexCoord,id:2855,x:27906,y:32624,varname:node_2855,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Append,id:9119,x:28872,y:32335,varname:node_9119,prsc:2|A-6528-OUT,B-2732-OUT;n:type:ShaderForge.SFN_Time,id:1981,x:27906,y:32203,varname:node_1981,prsc:2;n:type:ShaderForge.SFN_Add,id:6528,x:28489,y:32354,varname:node_6528,prsc:2|A-2986-OUT,B-2855-U,C-9574-B;n:type:ShaderForge.SFN_Add,id:2732,x:28440,y:32628,varname:node_2732,prsc:2|A-6103-OUT,B-2855-V;n:type:ShaderForge.SFN_Vector4Property,id:1493,x:27906,y:32420,ptovrint:False,ptlb:[MaskTeoxMove_xyz],ptin:_MaskTeoxMove_xyz,varname:_MaskTeoxMove_xyz,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Multiply,id:2986,x:28149,y:32260,varname:node_2986,prsc:2|A-1981-T,B-1493-X;n:type:ShaderForge.SFN_Multiply,id:6103,x:28149,y:32394,varname:node_6103,prsc:2|A-1981-T,B-1493-Y;n:type:ShaderForge.SFN_Rotator,id:7419,x:29526,y:32331,varname:node_7419,prsc:2|UVIN-9119-OUT,ANG-7665-OUT;n:type:ShaderForge.SFN_Pi,id:3895,x:28902,y:32740,varname:node_3895,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7665,x:29159,y:32640,varname:node_7665,prsc:2|A-3591-OUT,B-3895-OUT;n:type:ShaderForge.SFN_RemapRange,id:3591,x:28725,y:32592,varname:node_3591,prsc:2,frmn:0,frmx:360,tomn:0,tomx:2|IN-1493-Z;n:type:ShaderForge.SFN_Add,id:8218,x:30748,y:32524,varname:node_8218,prsc:2|A-8399-OUT,B-2231-OUT;n:type:ShaderForge.SFN_Tex2d,id:462,x:29121,y:34142,varname:_node_752,prsc:2,ntxv:0,isnm:False|UVIN-2523-OUT,TEX-8595-TEX;n:type:ShaderForge.SFN_Multiply,id:6579,x:31403,y:32578,varname:node_6579,prsc:2|A-8085-OUT,B-5763-A;n:type:ShaderForge.SFN_Append,id:6304,x:28234,y:34322,varname:node_6304,prsc:2|A-9269-OUT,B-6348-OUT;n:type:ShaderForge.SFN_Add,id:6348,x:27949,y:34488,varname:node_6348,prsc:2|A-8819-OUT,B-108-V;n:type:ShaderForge.SFN_Add,id:9269,x:27949,y:34261,varname:node_9269,prsc:2|A-5405-OUT,B-108-U;n:type:ShaderForge.SFN_Multiply,id:8819,x:27730,y:34384,varname:node_8819,prsc:2|A-6037-T,B-8589-Y;n:type:ShaderForge.SFN_Multiply,id:5405,x:27740,y:34151,varname:node_5405,prsc:2|A-6037-T,B-8589-X;n:type:ShaderForge.SFN_Multiply,id:7658,x:29578,y:34235,varname:node_7658,prsc:2|A-2572-OUT,B-462-A;n:type:ShaderForge.SFN_SwitchProperty,id:1923,x:33536,y:31852,ptovrint:False,ptlb:UserColor,ptin:_UserColor,varname:_UserColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-6344-OUT,B-8623-OUT;n:type:ShaderForge.SFN_Power,id:49,x:32164,y:31364,varname:node_49,prsc:2|VAL-6439-OUT,EXP-2207-OUT;n:type:ShaderForge.SFN_Slider,id:2207,x:31785,y:31386,ptovrint:False,ptlb:GrayExp,ptin:_GrayExp,varname:_GrayExp,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:4.982912,max:20;n:type:ShaderForge.SFN_Multiply,id:6219,x:33985,y:32767,varname:node_6219,prsc:2|A-1923-OUT,B-2662-RGB;n:type:ShaderForge.SFN_RemapRange,id:1117,x:31757,y:31991,varname:node_1117,prsc:2,frmn:0,frmx:1,tomn:0,tomx:0.95|IN-1835-OUT;n:type:ShaderForge.SFN_VertexColor,id:2662,x:33177,y:32779,varname:node_2662,prsc:2;n:type:ShaderForge.SFN_Clamp01,id:1300,x:33524,y:33194,varname:node_1300,prsc:2|IN-76-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:9152,x:30957,y:31978,ptovrint:False,ptlb:[MainTex],ptin:_MainTex,varname:_MainTex,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_ValueProperty,id:7363,x:27174,y:30124,ptovrint:False,ptlb:[MainTexUVScale],ptin:_MainTexUVScale,varname:_MainTexUVScale,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_TexCoord,id:5677,x:27187,y:29877,varname:node_5677,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:455,x:27626,y:29922,varname:node_455,prsc:2|A-5677-UVOUT,B-7363-OUT;n:type:ShaderForge.SFN_Multiply,id:2872,x:27421,y:30153,varname:node_2872,prsc:2|A-7363-OUT,B-703-OUT;n:type:ShaderForge.SFN_Vector1,id:703,x:27216,y:30329,varname:node_703,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Subtract,id:5820,x:27641,y:30235,varname:node_5820,prsc:2|A-2872-OUT,B-703-OUT;n:type:ShaderForge.SFN_Negate,id:1121,x:27806,y:30207,varname:node_1121,prsc:2|IN-5820-OUT;n:type:ShaderForge.SFN_Add,id:9260,x:27954,y:30010,varname:node_9260,prsc:2|A-455-OUT,B-1121-OUT;n:type:ShaderForge.SFN_ComponentMask,id:241,x:28174,y:30010,varname:node_241,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-9260-OUT;n:type:ShaderForge.SFN_Rotator,id:8866,x:28508,y:34319,varname:node_8866,prsc:2|UVIN-6304-OUT,SPD-8589-Z;n:type:ShaderForge.SFN_Vector4Property,id:8589,x:27318,y:34256,ptovrint:False,ptlb:NoiseSpeedSubTex,ptin:_NoiseSpeedSubTex,varname:_NoiseSpeedSubTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Append,id:4705,x:29851,y:33460,varname:node_4705,prsc:2|A-3824-OUT,B-3824-OUT;n:type:ShaderForge.SFN_Power,id:3824,x:29585,y:33453,varname:node_3824,prsc:2|VAL-767-OUT,EXP-9438-OUT;n:type:ShaderForge.SFN_Slider,id:9438,x:28963,y:33413,ptovrint:False,ptlb:NoiseExp,ptin:_NoiseExp,varname:_NoiseExp,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:20;n:type:ShaderForge.SFN_ConstantLerp,id:767,x:29395,y:33504,varname:node_767,prsc:2,a:0,b:0.98|IN-4719-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:2048,x:31482,y:32953,ptovrint:False,ptlb:[Negate],ptin:_Negate,varname:_Negate,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-6579-OUT,B-2717-OUT;n:type:ShaderForge.SFN_OneMinus,id:2717,x:31246,y:32974,varname:node_2717,prsc:2|IN-6579-OUT;n:type:ShaderForge.SFN_RemapRange,id:2410,x:31672,y:33183,varname:node_2410,prsc:2,frmn:0,frmx:1,tomn:0,tomx:20|IN-8692-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:2231,x:30425,y:33033,ptovrint:False,ptlb:UserNoiseForMask,ptin:_UserNoiseForMask,varname:_UserNoiseForMask,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-8847-OUT,B-699-OUT;n:type:ShaderForge.SFN_Vector1,id:8847,x:30072,y:32939,varname:node_8847,prsc:2,v1:0;n:type:ShaderForge.SFN_Desaturate,id:1539,x:29320,y:33811,varname:node_1539,prsc:2|COL-9497-RGB;n:type:ShaderForge.SFN_Multiply,id:5913,x:29506,y:33856,varname:node_5913,prsc:2|A-1539-OUT,B-9497-A;n:type:ShaderForge.SFN_Desaturate,id:2572,x:29330,y:34194,varname:node_2572,prsc:2|COL-462-RGB;n:type:ShaderForge.SFN_Multiply,id:3505,x:32327,y:32908,varname:node_3505,prsc:2|A-1451-OUT,B-3307-OUT,C-1274-OUT;n:type:ShaderForge.SFN_Color,id:1820,x:32278,y:30976,ptovrint:False,ptlb:Black,ptin:_Black,varname:_Black,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Lerp,id:8623,x:32867,y:31118,varname:node_8623,prsc:2|A-7608-OUT,B-4901-RGB,T-5721-OUT;n:type:ShaderForge.SFN_Power,id:5721,x:32720,y:31396,varname:node_5721,prsc:2|VAL-6439-OUT,EXP-5601-OUT;n:type:ShaderForge.SFN_Slider,id:5601,x:32279,y:31460,ptovrint:False,ptlb:LightExp,ptin:_LightExp,varname:_LightExp,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:6.881839,max:20;n:type:ShaderForge.SFN_Slider,id:1841,x:30675,y:31668,ptovrint:False,ptlb:[MainTexBrightExp],ptin:_MainTexBrightExp,varname:_MainTexBrightExp,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:60;n:type:ShaderForge.SFN_Tex2d,id:347,x:30864,y:31278,ptovrint:False,ptlb:SubTexture,ptin:_SubTexture,varname:_SubTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-342-OUT;n:type:ShaderForge.SFN_TexCoord,id:1491,x:28747,y:28874,varname:node_1491,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Append,id:7749,x:29713,y:28585,varname:node_7749,prsc:2|A-6043-OUT,B-9061-OUT;n:type:ShaderForge.SFN_Time,id:512,x:28747,y:28447,varname:node_512,prsc:2;n:type:ShaderForge.SFN_Add,id:6043,x:29330,y:28604,varname:node_6043,prsc:2|A-7398-OUT,B-1491-U,C-9574-G;n:type:ShaderForge.SFN_Add,id:9061,x:29281,y:28878,varname:node_9061,prsc:2|A-4195-OUT,B-1491-V;n:type:ShaderForge.SFN_Vector4Property,id:2194,x:28747,y:28670,ptovrint:False,ptlb:SubTexMove_xyz_copy,ptin:_SubTexMove_xyz_copy,varname:_SubTexMove_xyz_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Multiply,id:7398,x:28990,y:28471,varname:node_7398,prsc:2|A-512-T,B-2194-X;n:type:ShaderForge.SFN_Multiply,id:4195,x:28990,y:28644,varname:node_4195,prsc:2|A-512-T,B-2194-Y;n:type:ShaderForge.SFN_Rotator,id:9636,x:30233,y:28599,varname:node_9636,prsc:2|UVIN-7749-OUT,ANG-3704-OUT;n:type:ShaderForge.SFN_Pi,id:761,x:29743,y:28990,varname:node_761,prsc:2;n:type:ShaderForge.SFN_Multiply,id:3704,x:30000,y:28890,varname:node_3704,prsc:2|A-6521-OUT,B-761-OUT;n:type:ShaderForge.SFN_RemapRange,id:6521,x:29566,y:28842,varname:node_6521,prsc:2,frmn:0,frmx:360,tomn:0,tomx:2|IN-2194-Z;n:type:ShaderForge.SFN_Multiply,id:628,x:33946,y:32931,varname:node_628,prsc:2|A-2662-A,B-1300-OUT,C-411-OUT;n:type:ShaderForge.SFN_Multiply,id:6685,x:32843,y:31646,varname:node_6685,prsc:2|A-85-OUT,B-3361-OUT;n:type:ShaderForge.SFN_Multiply,id:1835,x:31552,y:31991,varname:node_1835,prsc:2|A-4081-OUT,B-476-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:1451,x:32029,y:32032,ptovrint:False,ptlb:[UserTexBrightAsAlpha],ptin:_UserTexBrightAsAlpha,varname:_UserTexBrightAsAlpha,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-1117-OUT,B-2399-OUT;n:type:ShaderForge.SFN_Multiply,id:2399,x:31968,y:31721,varname:node_2399,prsc:2|A-7761-OUT,B-8262-OUT,C-1117-OUT;n:type:ShaderForge.SFN_Multiply,id:2108,x:30078,y:34933,varname:node_2108,prsc:2|A-7227-OUT,B-8220-OUT;n:type:ShaderForge.SFN_Slider,id:7227,x:29685,y:34804,ptovrint:False,ptlb:NoiseAsSubTex,ptin:_NoiseAsSubTex,varname:_NoiseAsSubTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.06930693,max:1;n:type:ShaderForge.SFN_Append,id:8220,x:29860,y:34949,varname:node_8220,prsc:2|A-5237-OUT,B-5237-OUT;n:type:ShaderForge.SFN_Power,id:5237,x:29612,y:34961,varname:node_5237,prsc:2|VAL-9337-OUT,EXP-1060-OUT;n:type:ShaderForge.SFN_Slider,id:1060,x:29323,y:34792,ptovrint:False,ptlb:NoiseExpAsSubTex,ptin:_NoiseExpAsSubTex,varname:_NoiseExpAsSubTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:20;n:type:ShaderForge.SFN_ConstantLerp,id:9337,x:29402,y:34961,varname:node_9337,prsc:2,a:0,b:0.98|IN-9558-OUT;n:type:ShaderForge.SFN_Add,id:342,x:30616,y:31252,varname:node_342,prsc:2|A-913-OUT,B-2108-OUT;n:type:ShaderForge.SFN_Multiply,id:8808,x:32612,y:34617,varname:node_8808,prsc:2|A-9004-RGB,B-6675-OUT;n:type:ShaderForge.SFN_Color,id:9004,x:31965,y:34314,ptovrint:False,ptlb:LineColor,ptin:_LineColor,varname:_LineColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:0;n:type:ShaderForge.SFN_Subtract,id:562,x:31391,y:34711,varname:node_562,prsc:2|A-7144-OUT,B-4733-OUT;n:type:ShaderForge.SFN_Subtract,id:6675,x:32145,y:34714,varname:node_6675,prsc:2|A-2818-OUT,B-3110-OUT;n:type:ShaderForge.SFN_Ceil,id:3110,x:31933,y:34950,varname:node_3110,prsc:2|IN-5333-OUT;n:type:ShaderForge.SFN_Subtract,id:5333,x:31749,y:34950,varname:node_5333,prsc:2|A-3863-OUT,B-2221-OUT;n:type:ShaderForge.SFN_Ceil,id:2818,x:31749,y:34711,varname:node_2818,prsc:2|IN-3863-OUT;n:type:ShaderForge.SFN_Slider,id:3293,x:30753,y:35130,ptovrint:False,ptlb:ClipValue,ptin:_ClipValue,varname:_ClipValue,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-0.1,cur:0,max:1.1;n:type:ShaderForge.SFN_Add,id:4733,x:31117,y:34893,varname:node_4733,prsc:2|A-3359-A,B-3293-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2221,x:31461,y:35025,ptovrint:False,ptlb:ClipWideValue,ptin:_ClipWideValue,varname:_ClipWideValue,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.01;n:type:ShaderForge.SFN_Slider,id:4209,x:30577,y:30921,ptovrint:False,ptlb:SubTexBrightExp,ptin:_SubTexBrightExp,varname:_SubTexBrightExp,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:20;n:type:ShaderForge.SFN_Power,id:8262,x:31766,y:31739,varname:node_8262,prsc:2|VAL-9156-OUT,EXP-6531-OUT;n:type:ShaderForge.SFN_Power,id:7761,x:31438,y:31049,varname:node_7761,prsc:2|VAL-7018-OUT,EXP-2049-OUT;n:type:ShaderForge.SFN_Desaturate,id:769,x:31048,y:31108,varname:node_769,prsc:2|COL-347-RGB;n:type:ShaderForge.SFN_Desaturate,id:5,x:31415,y:31799,varname:node_5,prsc:2|COL-8312-RGB;n:type:ShaderForge.SFN_Power,id:1274,x:32083,y:32952,varname:node_1274,prsc:2|VAL-2048-OUT,EXP-240-OUT;n:type:ShaderForge.SFN_Clamp01,id:3863,x:31569,y:34711,varname:node_3863,prsc:2|IN-562-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:7018,x:31219,y:31108,varname:node_7018,prsc:2,min:0,max:0.98|IN-769-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:9156,x:31585,y:31799,varname:node_9156,prsc:2,min:0,max:0.98|IN-5-OUT;n:type:ShaderForge.SFN_Add,id:240,x:31920,y:33183,varname:node_240,prsc:2|A-2410-OUT,B-3359-B;n:type:ShaderForge.SFN_SwitchProperty,id:7144,x:31176,y:34688,ptovrint:False,ptlb:UserMainTexAsClip,ptin:_UserMainTexAsClip,varname:_UserMainTexAsClip,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-2048-OUT,B-1451-OUT;n:type:ShaderForge.SFN_TexCoord,id:230,x:26137,y:32402,varname:node_230,prsc:2,uv:1,uaff:True;n:type:ShaderForge.SFN_ComponentMask,id:9574,x:26974,y:32054,varname:node_9574,prsc:2,cc1:0,cc2:1,cc3:2,cc4:3|IN-3430-OUT;n:type:ShaderForge.SFN_Append,id:1537,x:26354,y:32402,varname:node_1537,prsc:2|A-230-U,B-230-V,C-230-Z,D-230-W;n:type:ShaderForge.SFN_Vector4,id:4767,x:26321,y:32099,varname:node_4767,prsc:2,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_SwitchProperty,id:3430,x:26513,y:32266,ptovrint:False,ptlb:[UserParticleValueAsSpeed],ptin:_UserParticleValueAsSpeed,varname:_UserParticleValueAsSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-4767-OUT,B-1537-OUT;n:type:ShaderForge.SFN_TexCoord,id:536,x:28226,y:33199,varname:node_536,prsc:2,uv:2,uaff:True;n:type:ShaderForge.SFN_ComponentMask,id:3359,x:28949,y:33107,varname:node_3359,prsc:2,cc1:0,cc2:1,cc3:2,cc4:3|IN-2569-OUT;n:type:ShaderForge.SFN_Append,id:4711,x:28533,y:33219,varname:node_4711,prsc:2|A-536-U,B-536-V,C-536-Z,D-536-W;n:type:ShaderForge.SFN_Vector4,id:9304,x:28521,y:32946,varname:node_9304,prsc:2,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_SwitchProperty,id:2569,x:28713,y:33119,ptovrint:False,ptlb:[UserParticleValueAsClip],ptin:_UserParticleValueAsClip,varname:_UserParticleValueAsClip,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-9304-OUT,B-4711-OUT;n:type:ShaderForge.SFN_Add,id:2049,x:30925,y:30959,varname:node_2049,prsc:2|A-4209-OUT,B-3359-G;n:type:ShaderForge.SFN_Add,id:6531,x:31174,y:31667,varname:node_6531,prsc:2|A-1841-OUT,B-3359-R;n:type:ShaderForge.SFN_SwitchProperty,id:6254,x:33219,y:34170,ptovrint:False,ptlb:OpenClip,ptin:_OpenClip,varname:_OpenClip,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-3913-OUT,B-2818-OUT;n:type:ShaderForge.SFN_Vector1,id:3913,x:32777,y:33982,varname:node_3913,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:6344,x:33332,y:31254,varname:node_6344,prsc:2|A-4901-RGB,B-6685-OUT;n:type:ShaderForge.SFN_Multiply,id:9926,x:34241,y:33268,varname:node_9926,prsc:2|A-2662-A,B-8808-OUT;n:type:ShaderForge.SFN_Multiply,id:1755,x:32360,y:34177,varname:node_1755,prsc:2|A-9004-A,B-6675-OUT;n:type:ShaderForge.SFN_Add,id:76,x:33305,y:33194,varname:node_76,prsc:2|A-8029-OUT,B-7346-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:3488,x:32532,y:33842,ptovrint:False,ptlb:AddClipAsAlpha,ptin:_AddClipAsAlpha,varname:_AddClipAsAlpha,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-4478-OUT,B-1755-OUT;n:type:ShaderForge.SFN_Vector1,id:4478,x:32186,y:33885,varname:node_4478,prsc:2,v1:0;n:type:ShaderForge.SFN_Power,id:476,x:31384,y:32028,varname:node_476,prsc:2|VAL-8312-A,EXP-6531-OUT;n:type:ShaderForge.SFN_Power,id:4081,x:31141,y:31355,varname:node_4081,prsc:2|VAL-347-A,EXP-2049-OUT;n:type:ShaderForge.SFN_Multiply,id:6439,x:31735,y:31102,varname:node_6439,prsc:2|A-7761-OUT,B-8262-OUT;n:type:ShaderForge.SFN_Clamp01,id:9640,x:34194,y:32931,varname:node_9640,prsc:2|IN-628-OUT;n:type:ShaderForge.SFN_TexCoord,id:8393,x:27974,y:30974,varname:node_8393,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Append,id:9398,x:28940,y:30685,varname:node_9398,prsc:2|A-1964-OUT,B-6684-OUT;n:type:ShaderForge.SFN_Time,id:185,x:27974,y:30547,varname:node_185,prsc:2;n:type:ShaderForge.SFN_Add,id:1964,x:28557,y:30704,varname:node_1964,prsc:2|A-1087-OUT,B-8393-U;n:type:ShaderForge.SFN_Add,id:6684,x:28508,y:30978,varname:node_6684,prsc:2|A-2003-OUT,B-8393-V;n:type:ShaderForge.SFN_Vector4Property,id:9036,x:27974,y:30770,ptovrint:False,ptlb:[SubMaskTeoxMove_xyz],ptin:_SubMaskTeoxMove_xyz,varname:_SubMaskTeoxMove_xyz,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Multiply,id:1087,x:28217,y:30571,varname:node_1087,prsc:2|A-185-T,B-9036-X;n:type:ShaderForge.SFN_Multiply,id:2003,x:28217,y:30744,varname:node_2003,prsc:2|A-185-T,B-9036-Y;n:type:ShaderForge.SFN_Rotator,id:5590,x:29478,y:30811,varname:node_5590,prsc:2|UVIN-9398-OUT,ANG-1695-OUT;n:type:ShaderForge.SFN_Pi,id:2162,x:28989,y:31002,varname:node_2162,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1695,x:29254,y:30894,varname:node_1695,prsc:2|A-9333-OUT,B-2162-OUT;n:type:ShaderForge.SFN_RemapRange,id:9333,x:28793,y:30942,varname:node_9333,prsc:2,frmn:0,frmx:360,tomn:0,tomx:2|IN-9036-Z;n:type:ShaderForge.SFN_Tex2d,id:6666,x:30926,y:32325,ptovrint:False,ptlb:[SubMaskTex],ptin:_SubMaskTex,varname:_SubMaskTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1294-OUT;n:type:ShaderForge.SFN_Add,id:1294,x:30674,y:32325,varname:node_1294,prsc:2|A-6116-OUT,B-5544-OUT;n:type:ShaderForge.SFN_Desaturate,id:9905,x:31179,y:32293,varname:node_9905,prsc:2|COL-6666-RGB;n:type:ShaderForge.SFN_Multiply,id:8360,x:31403,y:32368,varname:node_8360,prsc:2|A-9905-OUT,B-6666-A;n:type:ShaderForge.SFN_Slider,id:4842,x:31600,y:32543,ptovrint:False,ptlb:[SubMaskExp],ptin:_SubMaskExp,varname:_SubMaskExp,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:20;n:type:ShaderForge.SFN_Power,id:3307,x:31965,y:32374,varname:node_3307,prsc:2|VAL-8360-OUT,EXP-4842-OUT;n:type:ShaderForge.SFN_TexCoord,id:5985,x:27998,y:31430,varname:node_5985,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_RemapRange,id:303,x:28204,y:31430,varname:node_303,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-5985-UVOUT;n:type:ShaderForge.SFN_ArcTan2,id:2733,x:28731,y:31279,varname:node_2733,prsc:2,attp:2|A-2261-R,B-2261-G;n:type:ShaderForge.SFN_ComponentMask,id:2261,x:28476,y:31260,varname:node_2261,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-303-OUT;n:type:ShaderForge.SFN_Rotator,id:3691,x:29335,y:31379,varname:node_3691,prsc:2|UVIN-4087-OUT,ANG-1695-OUT;n:type:ShaderForge.SFN_Length,id:6494,x:28474,y:31513,varname:node_6494,prsc:2|IN-303-OUT;n:type:ShaderForge.SFN_Add,id:3611,x:28853,y:31528,varname:node_3611,prsc:2|A-6494-OUT,B-160-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8485,x:28470,y:31711,ptovrint:False,ptlb:[SubMaskTexSecUVSpeed],ptin:_SubMaskTexSecUVSpeed,varname:_SubMaskTexSecUVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:160,x:28661,y:31711,varname:node_160,prsc:2|A-8485-OUT,B-2240-T;n:type:ShaderForge.SFN_Time,id:2240,x:28474,y:31815,varname:node_2240,prsc:2;n:type:ShaderForge.SFN_Append,id:4087,x:29093,y:31379,varname:node_4087,prsc:2|A-2733-OUT,B-3611-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:6116,x:29732,y:31360,ptovrint:False,ptlb:[UserSubMaskTexSecUV],ptin:_UserSubMaskTexSecUV,varname:_UserSubMaskTexSecUV,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-5590-UVOUT,B-3691-UVOUT;n:type:ShaderForge.SFN_ValueProperty,id:4137,x:28474,y:32036,ptovrint:False,ptlb:[MaskTexSecUVSpeed],ptin:_MaskTexSecUVSpeed,varname:_MaskTexSecUVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:2664,x:28661,y:31946,varname:node_2664,prsc:2|A-2240-T,B-4137-OUT;n:type:ShaderForge.SFN_Add,id:144,x:28848,y:31926,varname:node_144,prsc:2|A-6494-OUT,B-2664-OUT,C-9574-B;n:type:ShaderForge.SFN_Append,id:6166,x:29117,y:31807,varname:node_6166,prsc:2|A-2733-OUT,B-144-OUT;n:type:ShaderForge.SFN_Rotator,id:3027,x:29373,y:31807,varname:node_3027,prsc:2|UVIN-6166-OUT,ANG-7665-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:8399,x:29742,y:31779,ptovrint:False,ptlb:[UserMaskTexSecUV],ptin:_UserMaskTexSecUV,varname:_UserMaskTexSecUV,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-7419-UVOUT,B-3027-UVOUT;n:type:ShaderForge.SFN_Multiply,id:2523,x:28759,y:34319,varname:node_2523,prsc:2|A-8866-UVOUT,B-2811-OUT;n:type:ShaderForge.SFN_Add,id:2811,x:28478,y:34460,varname:node_2811,prsc:2|A-8589-W,B-8560-OUT;n:type:ShaderForge.SFN_Vector1,id:8560,x:28268,y:34526,varname:node_8560,prsc:2,v1:1;n:type:ShaderForge.SFN_Add,id:3593,x:29117,y:30220,varname:node_3593,prsc:2|A-6494-OUT,B-9008-OUT,C-9574-R;n:type:ShaderForge.SFN_Append,id:9715,x:29429,y:30209,varname:node_9715,prsc:2|A-2733-OUT,B-3593-OUT;n:type:ShaderForge.SFN_Rotator,id:4105,x:29694,y:30206,varname:node_4105,prsc:2|UVIN-9715-OUT,ANG-2187-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:4198,x:30047,y:30195,ptovrint:False,ptlb:[UserMainTexSecUV],ptin:_UserMainTexSecUV,varname:_UserMainTexSecUV,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-7522-UVOUT,B-4105-UVOUT;n:type:ShaderForge.SFN_Multiply,id:9008,x:28874,y:30240,varname:node_9008,prsc:2|A-2240-T,B-1855-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1855,x:28639,y:30254,ptovrint:False,ptlb:[MainTexSecUVSpeed],ptin:_MainTexSecUVSpeed,varname:_MainTexSecUVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:7821,x:29540,y:29088,varname:node_7821,prsc:2|A-6494-OUT,B-890-OUT,C-9574-G;n:type:ShaderForge.SFN_Append,id:187,x:29865,y:29069,varname:node_187,prsc:2|A-2733-OUT,B-7821-OUT;n:type:ShaderForge.SFN_Rotator,id:176,x:30100,y:29069,varname:node_176,prsc:2|UVIN-187-OUT,ANG-3704-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:913,x:30483,y:29055,ptovrint:False,ptlb:UserSubTexSecUV,ptin:_UserSubTexSecUV,varname:_UserSubTexSecUV,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-9636-UVOUT,B-176-UVOUT;n:type:ShaderForge.SFN_Multiply,id:890,x:29310,y:29091,varname:node_890,prsc:2|A-2240-T,B-1079-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1079,x:29081,y:29101,ptovrint:False,ptlb:SubTexSecUVSpeed,ptin:_SubTexSecUVSpeed,varname:_SubTexSecUVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Slider,id:8661,x:32205,y:31853,ptovrint:False,ptlb:[MainTexDesaturate],ptin:_MainTexDesaturate,varname:_MainTexDesaturate,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Desaturate,id:3361,x:32627,y:31736,varname:node_3361,prsc:2|COL-8312-RGB,DES-8661-OUT;n:type:ShaderForge.SFN_Desaturate,id:85,x:32627,y:31602,varname:node_85,prsc:2|COL-347-RGB,DES-5129-OUT;n:type:ShaderForge.SFN_Slider,id:5129,x:32205,y:31655,ptovrint:False,ptlb:SubTexDesaturate,ptin:_SubTexDesaturate,varname:_SubTexDesaturate,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_SwitchProperty,id:5544,x:30425,y:32882,ptovrint:False,ptlb:UserNoiseForSubMask,ptin:_UserNoiseForSubMask,varname:_UserNoiseForSubMask,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-8847-OUT,B-699-OUT;n:type:ShaderForge.SFN_Fresnel,id:6133,x:32985,y:34931,varname:node_6133,prsc:2|EXP-7762-OUT;n:type:ShaderForge.SFN_Slider,id:7762,x:32549,y:34962,ptovrint:False,ptlb:[FresnelExp],ptin:_FresnelExp,varname:_FresnelExp,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:2;n:type:ShaderForge.SFN_OneMinus,id:5346,x:33241,y:34933,varname:node_5346,prsc:2|IN-6133-OUT;n:type:ShaderForge.SFN_Slider,id:3830,x:33146,y:34753,ptovrint:False,ptlb:[FresnelRange1],ptin:_FresnelRange1,varname:_FresnelRange1,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Clamp01,id:411,x:34038,y:34924,varname:node_411,prsc:2|IN-296-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:296,x:33661,y:34936,varname:node_296,prsc:2|IN-5346-OUT,IMIN-216-OUT,IMAX-3830-OUT,OMIN-1468-OUT,OMAX-6928-OUT;n:type:ShaderForge.SFN_Vector1,id:1468,x:33397,y:35181,varname:node_1468,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:6928,x:33438,y:35112,varname:node_6928,prsc:2,v1:1;n:type:ShaderForge.SFN_Slider,id:216,x:33146,y:34627,ptovrint:False,ptlb:[FresnelRange0],ptin:_FresnelRange0,varname:_FresnelRange0,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8234634,max:1;n:type:ShaderForge.SFN_ValueProperty,id:2124,x:27973,y:33445,ptovrint:False,ptlb:NoiseTexSecUVspeed,ptin:_NoiseTexSecUVspeed,varname:_NoiseTexSecUVspeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:4684,x:28541,y:33520,varname:node_4684,prsc:2|A-6494-OUT,B-865-OUT,C-9574-A;n:type:ShaderForge.SFN_Append,id:7085,x:28695,y:33478,varname:node_7085,prsc:2|A-2733-OUT,B-4684-OUT;n:type:ShaderForge.SFN_Multiply,id:865,x:28135,y:33515,varname:node_865,prsc:2|A-2124-OUT,B-6037-T;n:type:ShaderForge.SFN_Rotator,id:274,x:28907,y:33500,varname:node_274,prsc:2|UVIN-7085-OUT,ANG-5000-Z;n:type:ShaderForge.SFN_SwitchProperty,id:7166,x:28913,y:33789,ptovrint:False,ptlb:UserNoiseTexSecUV,ptin:_UserNoiseTexSecUV,varname:_UserNoiseTexSecUV,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-82-UVOUT,B-274-UVOUT;n:type:ShaderForge.SFN_Multiply,id:7346,x:33061,y:33510,varname:node_7346,prsc:2|A-3307-OUT,B-3488-OUT;n:type:ShaderForge.SFN_Tex2d,id:9009,x:29353,y:33986,ptovrint:False,ptlb:NoiseMask,ptin:_NoiseMask,varname:_NoiseMask,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-4469-OUT;n:type:ShaderForge.SFN_Multiply,id:9655,x:29727,y:34002,varname:node_9655,prsc:2|A-1347-OUT,B-9009-A;n:type:ShaderForge.SFN_Desaturate,id:1347,x:29558,y:33986,varname:node_1347,prsc:2|COL-9009-RGB;n:type:ShaderForge.SFN_SwitchProperty,id:7561,x:30057,y:33936,ptovrint:False,ptlb:NoiseMaskSwitch,ptin:_NoiseMaskSwitch,varname:_NoiseMaskSwitch,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-8150-OUT,B-813-OUT;n:type:ShaderForge.SFN_Vector1,id:8150,x:29783,y:33932,varname:node_8150,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:4719,x:29803,y:33748,varname:node_4719,prsc:2|A-5913-OUT,B-7561-OUT;n:type:ShaderForge.SFN_Multiply,id:9558,x:29925,y:34252,varname:node_9558,prsc:2|A-7561-OUT,B-7658-OUT;n:type:ShaderForge.SFN_Append,id:5569,x:28323,y:34927,varname:node_5569,prsc:2|A-627-OUT,B-546-OUT;n:type:ShaderForge.SFN_Add,id:546,x:28050,y:35098,varname:node_546,prsc:2|A-608-OUT,B-108-V;n:type:ShaderForge.SFN_Add,id:627,x:28050,y:34862,varname:node_627,prsc:2|A-1109-OUT,B-108-U;n:type:ShaderForge.SFN_Multiply,id:608,x:27819,y:34989,varname:node_608,prsc:2|A-6037-T,B-7475-Y;n:type:ShaderForge.SFN_Multiply,id:1109,x:27829,y:34756,varname:node_1109,prsc:2|A-6037-T,B-7475-X;n:type:ShaderForge.SFN_Rotator,id:3884,x:28597,y:34924,varname:node_3884,prsc:2|UVIN-5569-OUT,SPD-7475-Z;n:type:ShaderForge.SFN_Vector4Property,id:7475,x:27357,y:34863,ptovrint:False,ptlb:NoiseMaskSpeed,ptin:_NoiseMaskSpeed,varname:_NoiseMaskSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_SwitchProperty,id:4469,x:29024,y:34962,ptovrint:False,ptlb:NoisiMaskUserPolar,ptin:_NoisiMaskUserPolar,varname:_NoisiMaskUserPolar,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-3884-UVOUT,B-4708-OUT;n:type:ShaderForge.SFN_Append,id:4708,x:28954,y:34763,varname:node_4708,prsc:2|A-2733-OUT,B-4806-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4754,x:29472,y:34439,ptovrint:False,ptlb:NoisiMaskPro,ptin:_NoisiMaskPro,varname:_NoisiMaskPro,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:3248,x:28510,y:34809,varname:node_3248,prsc:2|A-7475-X,B-6037-T;n:type:ShaderForge.SFN_Add,id:4806,x:28758,y:34753,varname:node_4806,prsc:2|A-6494-OUT,B-3248-OUT;n:type:ShaderForge.SFN_Power,id:813,x:29902,y:34017,varname:node_813,prsc:2|VAL-9655-OUT,EXP-4754-OUT;proporder:4901-7834-1820-5601-2207-1923-1841-8661-9152-7363-2802-4198-1855-4209-5129-347-2194-913-1079-5763-1493-2048-432-8399-4137-6666-9036-4842-6116-8485-8595-2231-5544-1248-9438-5000-7166-2124-8589-7227-1060-9009-7561-7475-4469-4754-1451-92-6254-7144-9004-3293-2221-3488-7762-216-3830-3430-2569;pass:END;sub:END;*/

Shader "BF/Effect/A/AParticleFireClip10" {
    Properties {
        [HDR]_Light ("Light", Color) = (1,1,1,1)
        _Gray ("Gray", Color) = (0.490566,0.490566,0.490566,1)
        _Black ("Black", Color) = (0,0,0,1)
        _LightExp ("LightExp", Range(0, 20)) = 6.881839
        _GrayExp ("GrayExp", Range(0, 20)) = 4.982912
        [MaterialToggle] _UserColor ("UserColor", Float ) = 0
        _MainTexBrightExp ("[MainTexBrightExp]", Range(0, 60)) = 1
        _MainTexDesaturate ("[MainTexDesaturate]", Range(0, 1)) = 0
        _MainTex ("[MainTex]", 2D) = "white" {}
        _MainTexUVScale ("[MainTexUVScale]", Float ) = 1
        _MainTexMove ("[MainTexMove]", Vector) = (0,0,0,0)
        [MaterialToggle] _UserMainTexSecUV ("[UserMainTexSecUV]", Float ) = 0
        _MainTexSecUVSpeed ("[MainTexSecUVSpeed]", Float ) = 0
        _SubTexBrightExp ("SubTexBrightExp", Range(0, 20)) = 1
        _SubTexDesaturate ("SubTexDesaturate", Range(0, 1)) = 0
        _SubTexture ("SubTexture", 2D) = "white" {}
        _SubTexMove_xyz_copy ("SubTexMove_xyz_copy", Vector) = (0,0,0,0)
        [MaterialToggle] _UserSubTexSecUV ("UserSubTexSecUV", Float ) = 0
        _SubTexSecUVSpeed ("SubTexSecUVSpeed", Float ) = 0
        _MaskTex ("[MaskTex]", 2D) = "white" {}
        _MaskTeoxMove_xyz ("[MaskTeoxMove_xyz]", Vector) = (0,0,0,0)
        [MaterialToggle] _Negate ("[Negate]", Float ) = 0
        _MaskExp ("[MaskExp]", Range(1, 0)) = 0.6939077
        [MaterialToggle] _UserMaskTexSecUV ("[UserMaskTexSecUV]", Float ) = 0
        _MaskTexSecUVSpeed ("[MaskTexSecUVSpeed]", Float ) = 0
        _SubMaskTex ("[SubMaskTex]", 2D) = "white" {}
        _SubMaskTeoxMove_xyz ("[SubMaskTeoxMove_xyz]", Vector) = (0,0,0,0)
        _SubMaskExp ("[SubMaskExp]", Range(0, 20)) = 1
        [MaterialToggle] _UserSubMaskTexSecUV ("[UserSubMaskTexSecUV]", Float ) = 0
        _SubMaskTexSecUVSpeed ("[SubMaskTexSecUVSpeed]", Float ) = 0
        _NoiseTex ("NoiseTex", 2D) = "white" {}
        [MaterialToggle] _UserNoiseForMask ("UserNoiseForMask", Float ) = 0
        [MaterialToggle] _UserNoiseForSubMask ("UserNoiseForSubMask", Float ) = 0
        _Noise ("Noise", Range(0, 1)) = 0
        _NoiseExp ("NoiseExp", Range(0, 20)) = 1
        _NoiseSpeed ("NoiseSpeed", Vector) = (0,0,0,0)
        [MaterialToggle] _UserNoiseTexSecUV ("UserNoiseTexSecUV", Float ) = 0.229402
        _NoiseTexSecUVspeed ("NoiseTexSecUVspeed", Float ) = 0
        _NoiseSpeedSubTex ("NoiseSpeedSubTex", Vector) = (0,0,0,0)
        _NoiseAsSubTex ("NoiseAsSubTex", Range(0, 1)) = 0.06930693
        _NoiseExpAsSubTex ("NoiseExpAsSubTex", Range(0, 20)) = 1
        _NoiseMask ("NoiseMask", 2D) = "white" {}
        [MaterialToggle] _NoiseMaskSwitch ("NoiseMaskSwitch", Float ) = 1
        _NoiseMaskSpeed ("NoiseMaskSpeed", Vector) = (0,0,0,0)
        [MaterialToggle] _NoisiMaskUserPolar ("NoisiMaskUserPolar", Float ) = 0.229402
        _NoisiMaskPro ("NoisiMaskPro", Float ) = 1
        [MaterialToggle] _UserTexBrightAsAlpha ("[UserTexBrightAsAlpha]", Float ) = 0
        _AlphaScale ("[AlphaScale]", Range(0, 10)) = 1
        [MaterialToggle] _OpenClip ("OpenClip", Float ) = 1
        [MaterialToggle] _UserMainTexAsClip ("UserMainTexAsClip", Float ) = 0
        [HDR]_LineColor ("LineColor", Color) = (0,0,0,0)
        _ClipValue ("ClipValue", Range(-0.1, 1.1)) = 0
        _ClipWideValue ("ClipWideValue", Float ) = 0.01
        [MaterialToggle] _AddClipAsAlpha ("AddClipAsAlpha", Float ) = 0
        _FresnelExp ("[FresnelExp]", Range(0, 2)) = 1
        _FresnelRange0 ("[FresnelRange0]", Range(0, 1)) = 0.8234634
        _FresnelRange1 ("[FresnelRange1]", Range(0, 1)) = 1
        [MaterialToggle] _UserParticleValueAsSpeed ("[UserParticleValueAsSpeed]", Float ) = 0
        [MaterialToggle] _UserParticleValueAsClip ("[UserParticleValueAsClip]", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform float4 _Light;
            uniform sampler2D _MaskTex; uniform float4 _MaskTex_ST;
            uniform float _MaskExp;
            uniform float4 _Gray;
            uniform float4 _NoiseSpeed;
            uniform float _Noise;
            uniform sampler2D _NoiseTex; uniform float4 _NoiseTex_ST;
            uniform float4 _MainTexMove;
            uniform float _AlphaScale;
            uniform float4 _MaskTeoxMove_xyz;
            uniform fixed _UserColor;
            uniform float _GrayExp;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _MainTexUVScale;
            uniform float4 _NoiseSpeedSubTex;
            uniform float _NoiseExp;
            uniform fixed _Negate;
            uniform fixed _UserNoiseForMask;
            uniform float4 _Black;
            uniform float _LightExp;
            uniform float _MainTexBrightExp;
            uniform sampler2D _SubTexture; uniform float4 _SubTexture_ST;
            uniform float4 _SubTexMove_xyz_copy;
            uniform fixed _UserTexBrightAsAlpha;
            uniform float _NoiseAsSubTex;
            uniform float _NoiseExpAsSubTex;
            uniform float4 _LineColor;
            uniform float _ClipValue;
            uniform float _ClipWideValue;
            uniform float _SubTexBrightExp;
            uniform fixed _UserMainTexAsClip;
            uniform fixed _UserParticleValueAsSpeed;
            uniform fixed _UserParticleValueAsClip;
            uniform fixed _OpenClip;
            uniform fixed _AddClipAsAlpha;
            uniform float4 _SubMaskTeoxMove_xyz;
            uniform sampler2D _SubMaskTex; uniform float4 _SubMaskTex_ST;
            uniform float _SubMaskExp;
            uniform float _SubMaskTexSecUVSpeed;
            uniform fixed _UserSubMaskTexSecUV;
            uniform float _MaskTexSecUVSpeed;
            uniform fixed _UserMaskTexSecUV;
            uniform fixed _UserMainTexSecUV;
            uniform float _MainTexSecUVSpeed;
            uniform fixed _UserSubTexSecUV;
            uniform float _SubTexSecUVSpeed;
            uniform float _MainTexDesaturate;
            uniform float _SubTexDesaturate;
            uniform fixed _UserNoiseForSubMask;
            uniform float _FresnelExp;
            uniform float _FresnelRange1;
            uniform float _FresnelRange0;
            uniform float _NoiseTexSecUVspeed;
            uniform fixed _UserNoiseTexSecUV;
            uniform sampler2D _NoiseMask; uniform float4 _NoiseMask_ST;
            uniform fixed _NoiseMaskSwitch;
            uniform float4 _NoiseMaskSpeed;
            uniform fixed _NoisiMaskUserPolar;
            uniform float _NoisiMaskPro;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float node_7665 = ((_MaskTeoxMove_xyz.b*0.005555556+0.0)*3.141592654);
                float node_7419_ang = node_7665;
                float node_7419_spd = 1.0;
                float node_7419_cos = cos(node_7419_spd*node_7419_ang);
                float node_7419_sin = sin(node_7419_spd*node_7419_ang);
                float2 node_7419_piv = float2(0.5,0.5);
                float4 node_1981 = _Time;
                float4 node_9574 = lerp( float4(0,0,0,0), float4(i.uv1.r,i.uv1.g,i.uv1.b,i.uv1.a), _UserParticleValueAsSpeed ).rgba;
                float2 node_7419 = (mul(float2(((node_1981.g*_MaskTeoxMove_xyz.r)+i.uv0.r+node_9574.b),((node_1981.g*_MaskTeoxMove_xyz.g)+i.uv0.g))-node_7419_piv,float2x2( node_7419_cos, -node_7419_sin, node_7419_sin, node_7419_cos))+node_7419_piv);
                float node_3027_ang = node_7665;
                float node_3027_spd = 1.0;
                float node_3027_cos = cos(node_3027_spd*node_3027_ang);
                float node_3027_sin = sin(node_3027_spd*node_3027_ang);
                float2 node_3027_piv = float2(0.5,0.5);
                float2 node_303 = (i.uv0*2.0+-1.0);
                float2 node_2261 = node_303.rg;
                float node_2733 = ((atan2(node_2261.r,node_2261.g)/6.28318530718)+0.5);
                float node_6494 = length(node_303);
                float4 node_2240 = _Time;
                float2 node_3027 = (mul(float2(node_2733,(node_6494+(node_2240.g*_MaskTexSecUVSpeed)+node_9574.b))-node_3027_piv,float2x2( node_3027_cos, -node_3027_sin, node_3027_sin, node_3027_cos))+node_3027_piv);
                float node_8847 = 0.0;
                float4 node_6456 = _Time;
                float node_82_ang = node_6456.g;
                float node_82_spd = _NoiseSpeed.b;
                float node_82_cos = cos(node_82_spd*node_82_ang);
                float node_82_sin = sin(node_82_spd*node_82_ang);
                float2 node_82_piv = float2(0.5,0.5);
                float4 node_6037 = _Time;
                float2 node_82 = (mul(float2(((node_6037.g*_NoiseSpeed.r)+i.uv0.r+node_9574.a),((node_6037.g*_NoiseSpeed.g)+i.uv0.g))-node_82_piv,float2x2( node_82_cos, -node_82_sin, node_82_sin, node_82_cos))+node_82_piv);
                float node_274_ang = _NoiseSpeed.b;
                float node_274_spd = 1.0;
                float node_274_cos = cos(node_274_spd*node_274_ang);
                float node_274_sin = sin(node_274_spd*node_274_ang);
                float2 node_274_piv = float2(0.5,0.5);
                float2 node_274 = (mul(float2(node_2733,(node_6494+(_NoiseTexSecUVspeed*node_6037.g)+node_9574.a))-node_274_piv,float2x2( node_274_cos, -node_274_sin, node_274_sin, node_274_cos))+node_274_piv);
                float2 _UserNoiseTexSecUV_var = lerp( node_82, node_274, _UserNoiseTexSecUV );
                float4 node_9497 = tex2D(_NoiseTex,TRANSFORM_TEX(_UserNoiseTexSecUV_var, _NoiseTex));
                float node_3884_ang = node_6456.g;
                float node_3884_spd = _NoiseMaskSpeed.b;
                float node_3884_cos = cos(node_3884_spd*node_3884_ang);
                float node_3884_sin = sin(node_3884_spd*node_3884_ang);
                float2 node_3884_piv = float2(0.5,0.5);
                float2 node_3884 = (mul(float2(((node_6037.g*_NoiseMaskSpeed.r)+i.uv0.r),((node_6037.g*_NoiseMaskSpeed.g)+i.uv0.g))-node_3884_piv,float2x2( node_3884_cos, -node_3884_sin, node_3884_sin, node_3884_cos))+node_3884_piv);
                float2 _NoisiMaskUserPolar_var = lerp( node_3884, float2(node_2733,(node_6494+(_NoiseMaskSpeed.r*node_6037.g))), _NoisiMaskUserPolar );
                float4 _NoiseMask_var = tex2D(_NoiseMask,TRANSFORM_TEX(_NoisiMaskUserPolar_var, _NoiseMask));
                float _NoiseMaskSwitch_var = lerp( 1.0, pow((dot(_NoiseMask_var.rgb,float3(0.3,0.59,0.11))*_NoiseMask_var.a),_NoisiMaskPro), _NoiseMaskSwitch );
                float node_3824 = pow(lerp(0,0.98,((dot(node_9497.rgb,float3(0.3,0.59,0.11))*node_9497.a)*_NoiseMaskSwitch_var)),_NoiseExp);
                float2 node_699 = (_Noise*float2(node_3824,node_3824));
                float2 node_8218 = (lerp( node_7419, node_3027, _UserMaskTexSecUV )+lerp( node_8847, node_699, _UserNoiseForMask ));
                float4 _MaskTex_var = tex2D(_MaskTex,TRANSFORM_TEX(node_8218, _MaskTex));
                float node_6579 = (dot(_MaskTex_var.rgb,float3(0.3,0.59,0.11))*_MaskTex_var.a);
                float _Negate_var = lerp( node_6579, (1.0 - node_6579), _Negate );
                float node_3704 = ((_SubTexMove_xyz_copy.b*0.005555556+0.0)*3.141592654);
                float node_9636_ang = node_3704;
                float node_9636_spd = 1.0;
                float node_9636_cos = cos(node_9636_spd*node_9636_ang);
                float node_9636_sin = sin(node_9636_spd*node_9636_ang);
                float2 node_9636_piv = float2(0.5,0.5);
                float4 node_512 = _Time;
                float2 node_9636 = (mul(float2(((node_512.g*_SubTexMove_xyz_copy.r)+i.uv0.r+node_9574.g),((node_512.g*_SubTexMove_xyz_copy.g)+i.uv0.g))-node_9636_piv,float2x2( node_9636_cos, -node_9636_sin, node_9636_sin, node_9636_cos))+node_9636_piv);
                float node_176_ang = node_3704;
                float node_176_spd = 1.0;
                float node_176_cos = cos(node_176_spd*node_176_ang);
                float node_176_sin = sin(node_176_spd*node_176_ang);
                float2 node_176_piv = float2(0.5,0.5);
                float2 node_176 = (mul(float2(node_2733,(node_6494+(node_2240.g*_SubTexSecUVSpeed)+node_9574.g))-node_176_piv,float2x2( node_176_cos, -node_176_sin, node_176_sin, node_176_cos))+node_176_piv);
                float node_8866_ang = node_6456.g;
                float node_8866_spd = _NoiseSpeedSubTex.b;
                float node_8866_cos = cos(node_8866_spd*node_8866_ang);
                float node_8866_sin = sin(node_8866_spd*node_8866_ang);
                float2 node_8866_piv = float2(0.5,0.5);
                float2 node_8866 = (mul(float2(((node_6037.g*_NoiseSpeedSubTex.r)+i.uv0.r),((node_6037.g*_NoiseSpeedSubTex.g)+i.uv0.g))-node_8866_piv,float2x2( node_8866_cos, -node_8866_sin, node_8866_sin, node_8866_cos))+node_8866_piv);
                float2 node_2523 = (node_8866*(_NoiseSpeedSubTex.a+1.0));
                float4 _node_752 = tex2D(_NoiseTex,TRANSFORM_TEX(node_2523, _NoiseTex));
                float node_5237 = pow(lerp(0,0.98,(_NoiseMaskSwitch_var*(dot(_node_752.rgb,float3(0.3,0.59,0.11))*_node_752.a))),_NoiseExpAsSubTex);
                float2 node_342 = (lerp( node_9636, node_176, _UserSubTexSecUV )+(_NoiseAsSubTex*float2(node_5237,node_5237)));
                float4 _SubTexture_var = tex2D(_SubTexture,TRANSFORM_TEX(node_342, _SubTexture));
                float4 node_3359 = lerp( float4(0,0,0,0), float4(i.uv2.r,i.uv2.g,i.uv2.b,i.uv2.a), _UserParticleValueAsClip ).rgba;
                float node_2049 = (_SubTexBrightExp+node_3359.g);
                float node_2187 = ((_MainTexMove.b*0.005555556+0.0)*3.141592654);
                float node_7522_ang = node_2187;
                float node_7522_spd = 1.0;
                float node_7522_cos = cos(node_7522_spd*node_7522_ang);
                float node_7522_sin = sin(node_7522_spd*node_7522_ang);
                float2 node_7522_piv = float2(0.5,0.5);
                float4 node_5254 = _Time;
                float node_703 = 0.5;
                float2 node_241 = ((i.uv0*_MainTexUVScale)+(-1*((_MainTexUVScale*node_703)-node_703))).rg;
                float2 node_7522 = (mul(float2(((node_5254.g*_MainTexMove.r)+node_241.r+node_9574.r),((node_5254.g*_MainTexMove.g)+node_241.g))-node_7522_piv,float2x2( node_7522_cos, -node_7522_sin, node_7522_sin, node_7522_cos))+node_7522_piv);
                float node_4105_ang = node_2187;
                float node_4105_spd = 1.0;
                float node_4105_cos = cos(node_4105_spd*node_4105_ang);
                float node_4105_sin = sin(node_4105_spd*node_4105_ang);
                float2 node_4105_piv = float2(0.5,0.5);
                float2 node_4105 = (mul(float2(node_2733,(node_6494+(node_2240.g*_MainTexSecUVSpeed)+node_9574.r))-node_4105_piv,float2x2( node_4105_cos, -node_4105_sin, node_4105_sin, node_4105_cos))+node_4105_piv);
                float2 node_89 = (lerp( node_7522, node_4105, _UserMainTexSecUV )+node_699);
                float4 _MainTexaa = tex2D(_MainTex,TRANSFORM_TEX(node_89, _MainTex));
                float node_6531 = (_MainTexBrightExp+node_3359.r);
                float node_1117 = ((pow(_SubTexture_var.a,node_2049)*pow(_MainTexaa.a,node_6531))*0.95+0.0);
                float node_7761 = pow(clamp(dot(_SubTexture_var.rgb,float3(0.3,0.59,0.11)),0,0.98),node_2049);
                float node_8262 = pow(clamp(dot(_MainTexaa.rgb,float3(0.3,0.59,0.11)),0,0.98),node_6531);
                float _UserTexBrightAsAlpha_var = lerp( node_1117, (node_7761*node_8262*node_1117), _UserTexBrightAsAlpha );
                float node_3863 = saturate((lerp( _Negate_var, _UserTexBrightAsAlpha_var, _UserMainTexAsClip )-(node_3359.a+_ClipValue)));
                float node_2818 = ceil(node_3863);
                clip(lerp( 1.0, node_2818, _OpenClip ) - 0.5);
////// Lighting:
////// Emissive:
                float node_6675 = (node_2818-ceil((node_3863-_ClipWideValue)));
                float3 emissive = (i.vertexColor.a*(_LineColor.rgb*node_6675));
                float node_6439 = (node_7761*node_8262);
                float3 finalColor = emissive + (lerp( (_Light.rgb*(lerp(_SubTexture_var.rgb,dot(_SubTexture_var.rgb,float3(0.3,0.59,0.11)),_SubTexDesaturate)*lerp(_MainTexaa.rgb,dot(_MainTexaa.rgb,float3(0.3,0.59,0.11)),_MainTexDesaturate))), lerp(lerp(_Black.rgb,_Gray.rgb,pow(node_6439,_GrayExp)),_Light.rgb,pow(node_6439,_LightExp)), _UserColor )*i.vertexColor.rgb);
                float node_1695 = ((_SubMaskTeoxMove_xyz.b*0.005555556+0.0)*3.141592654);
                float node_5590_ang = node_1695;
                float node_5590_spd = 1.0;
                float node_5590_cos = cos(node_5590_spd*node_5590_ang);
                float node_5590_sin = sin(node_5590_spd*node_5590_ang);
                float2 node_5590_piv = float2(0.5,0.5);
                float4 node_185 = _Time;
                float2 node_5590 = (mul(float2(((node_185.g*_SubMaskTeoxMove_xyz.r)+i.uv0.r),((node_185.g*_SubMaskTeoxMove_xyz.g)+i.uv0.g))-node_5590_piv,float2x2( node_5590_cos, -node_5590_sin, node_5590_sin, node_5590_cos))+node_5590_piv);
                float node_3691_ang = node_1695;
                float node_3691_spd = 1.0;
                float node_3691_cos = cos(node_3691_spd*node_3691_ang);
                float node_3691_sin = sin(node_3691_spd*node_3691_ang);
                float2 node_3691_piv = float2(0.5,0.5);
                float2 node_3691 = (mul(float2(node_2733,(node_6494+(_SubMaskTexSecUVSpeed*node_2240.g)))-node_3691_piv,float2x2( node_3691_cos, -node_3691_sin, node_3691_sin, node_3691_cos))+node_3691_piv);
                float2 node_1294 = (lerp( node_5590, node_3691, _UserSubMaskTexSecUV )+lerp( node_8847, node_699, _UserNoiseForSubMask ));
                float4 _SubMaskTex_var = tex2D(_SubMaskTex,TRANSFORM_TEX(node_1294, _SubMaskTex));
                float node_3307 = pow((dot(_SubMaskTex_var.rgb,float3(0.3,0.59,0.11))*_SubMaskTex_var.a),_SubMaskExp);
                float node_1468 = 0.0;
                return fixed4(finalColor,saturate((i.vertexColor.a*saturate((((_UserTexBrightAsAlpha_var*node_3307*pow(_Negate_var,(((_MaskExp*-1.0+1.0)*20.0+0.0)+node_3359.b)))*_AlphaScale)+(node_3307*lerp( 0.0, (_LineColor.a*node_6675), _AddClipAsAlpha ))))*saturate((node_1468 + ( ((1.0 - pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelExp)) - _FresnelRange0) * (1.0 - node_1468) ) / (_FresnelRange1 - _FresnelRange0))))));
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
