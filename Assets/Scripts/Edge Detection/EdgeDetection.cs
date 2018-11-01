using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeDetection : MonoBehaviour {

    [SerializeField] private Texture2D m_texture = null;
    [Range(0.1f, 3.0f)] [SerializeField] private int m_gaussianWeight = 1;
    [Range(0.1f, 3.0f)] [SerializeField] private int m_edgeWeight = 1;

    private Texture2D m_preview = null;
    private Renderer m_render = null;

	// Use this for initialization
	void Start () {
        m_render = GetComponent<Renderer>();
        m_preview = TextureFilter.Grayscale(m_texture);
        m_preview = TextureFilter.Convolution(m_preview, TextureFilter.GAUSSIAN_KERNEL_5, m_gaussianWeight);
        m_preview = TextureFilter.Convolution(m_preview, TextureFilter.EDGEDETECT_KERNEL_3, m_edgeWeight);
        m_render.material.mainTexture = m_preview;
    }

    private void OnValidate()
    {
        if (m_render == null || m_texture == null) return;

        m_preview = TextureFilter.Grayscale(m_texture);
        m_preview = TextureFilter.Convolution(m_preview, TextureFilter.GAUSSIAN_KERNEL_5, m_gaussianWeight);
        m_preview = TextureFilter.Convolution(m_preview, TextureFilter.EDGEDETECT_KERNEL_3, m_edgeWeight);
        m_render.material.mainTexture = m_preview;
    }
}
