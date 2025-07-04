Shader"Custom/WhiteOverlay" // 1. ���̴� �̸� ����
{
    Properties // 2. ������Ƽ ���.
    { // �ν����� â���� ����ڰ� ���� ���� ������ �� �ִ� �������� �����ϴ� ��
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Blend ("Blend", Range(0,1)) = 0
        _Alpha ("Alpha", Range(0,1)) = 1
    }
    SubShader // 3. ������̴� ���. ���� ������ ������ ���� �κ�
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"} // 4. �±�
        Blend SrcAlpha OneMinusSrcAlpha // 5. ���� ���
        Cull Off // 6. �ø�. �������� ��� �� ���� �׸��� ����. Off�� �ո� �޸� ��� �׸���
        Lighting Off // 7. ������. ���� ����Ʈ(����)�� ������ ������ ����
        ZWrite Off // 8. Z-���� ����. ���� ����(Z-buffer)�� �ڽ��� ���� ���� ���� ���� ����

        Pass // �н� ���
        {
            CGPROGRAM // 10. CG/HLSL �ڵ� ����
                #pragma vertex vert       // 11. ���ؽ� ���̴� �Լ� ����
                #pragma fragment frag     // 12. �����׸�Ʈ(�ȼ�) ���̴� �Լ� ����
                #include "UnityCG.cginc" // 13. ����Ƽ �⺻ �Լ� ����

                // 14. ���� ����
                sampler2D _MainTex;
                float _Blend;
                float _Alpha;
                float4 _MainTex_ST; // �ؽ�ó Ÿ�ϸ�/������ ����

                // 15. �Է� ����ü
                struct appdata_t
                {   
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                // 16. ���� -> �ȼ� ���� ����ü
                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                // 17. ���ؽ� ���̴�
                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    return o;
}

                // 18. �����׸�Ʈ ���̴�
                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv);
                    col.rgb = lerp(col.rgb, 1.0, _Blend); // White ����
                    col.a *= _Alpha; // ���İ��� ���� ����
                    return col;
                }
            ENDCG // 19. CG/HLSL �ڵ� ��
        }
    }
}
