// Compiled shader for PC, Mac & Linux Standalone, uncompressed size: 3.0KB

Shader "ZOnTop/ZOnTop" {
	Properties {
		 _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		 _MainTex ("Particle Texture", 2D) = "white" {}
		 _InvFade ("Soft Particles Factor", Range(0.01,3)) = 1
	}
	SubShader { 
		
		Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
		Pass {
			Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
			ZTest Always
			ZWrite Off
			Cull Off
			Fog {
				Color (0,0,0,1)
			}
			Blend One OneMinusSrcAlpha
			ColorMask RGB


			
			
	  
		}
	}
	

	


}