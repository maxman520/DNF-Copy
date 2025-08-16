using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 오디오 전역 관리 매니저(싱글턴).
/// - 효과음(SFX) 풀링 재생
/// - BGM A/B 이중 소스 크로스페이드
/// - 문자열 재생은 AudioLibrary 키 기반 조회
/// 
/// 사용 요약
/// - SFX: PlaySFX("Elven_Guard", 0.9f, 1f) 또는 PlaySFX(clip)
/// - BGM: PlayBGM("Character_Select", true, 1.0f) / StopBGM(0.3f)
/// - 볼륨: SetBgmVolume(0.6f), SetSfxVolume(0.8f), MuteAll(true)
/// </summary>
public sealed class AudioManager : Singleton<AudioManager>
{
        

        [Header("볼륨 (0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float bgmVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private bool muted = false;

        [Header("SFX 풀링")]
        [SerializeField] private int initialSfxPoolSize = 8;
        [SerializeField] private bool sfxAutoExpand = true;

        [Header("오디오 라이브러리")]
        [Tooltip("문자열 재생 시 참조되는 라이브러리.")]
        [SerializeField] private AudioLibrary audioLibrary;

        private readonly List<AudioSource> sfxPool = new List<AudioSource>();
        

        private AudioSource bgmA;
        private AudioSource bgmB;
        private AudioSource activeBgm;
        private AudioSource inactiveBgm;

        private CancellationTokenSource bgmCts;

        /// <summary>
        /// 싱글턴 초기화 및 내부 오디오 소스/풀을 설정합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            InitializeBgmSources();
            InitializeSfxPool();
            ApplyVolumes();
            ApplyMute();
        }

        /// <summary>
        /// BGM 전용 AudioSource(A/B)를 생성하고 초기화합니다.
        /// </summary>
        private void InitializeBgmSources()
        {
            // BGM 이중 소스 구성 (A/B)
            bgmA = CreateChildAudioSource("BGM_A");
            bgmB = CreateChildAudioSource("BGM_B");

            bgmA.loop = true;
            bgmB.loop = true;

            bgmA.playOnAwake = false;
            bgmB.playOnAwake = false;

            bgmA.volume = 0f;
            bgmB.volume = 0f;

            activeBgm = bgmA;
            inactiveBgm = bgmB;
        }

        /// <summary>
        /// SFX 재생을 위한 AudioSource 풀을 초기 크기만큼 준비합니다.
        /// </summary>
        private void InitializeSfxPool()
        {
            var sfxRoot = new GameObject("SFX_Pool");
            sfxRoot.transform.SetParent(transform);

            for (int i = 0; i < initialSfxPoolSize; i++)
            {
                var src = sfxRoot.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.volume = sfxVolume;
                sfxPool.Add(src);
            }
        }

        /// <summary>
        /// 자식 GameObject를 만들고 AudioSource를 부착해 반환합니다.
        /// </summary>
        private AudioSource CreateChildAudioSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            return src;
        }

        /// <summary>
        /// 현재 SFX 볼륨 값을 풀 내 모든 SFX 소스에 반영합니다.
        /// </summary>
        private void ApplyVolumes()
        {
            // BGM은 현재 값 유지(페이드/현 볼륨)에 스케일로 적용하기 위해 직접 세팅하지 않음
            // 대신 목표 볼륨 계산 시 bgmVolume 사용
            foreach (var src in sfxPool)
            {
                src.volume = sfxVolume;
            }
        }

        /// <summary>
        /// 현재 뮤트 값을 모든 오디오 소스에 반영합니다.
        /// </summary>
        private void ApplyMute()
        {
            var mute = muted;
            bgmA.mute = mute;
            bgmB.mute = mute;
            foreach (var src in sfxPool)
            {
                src.mute = mute;
            }
        }

        #region Public API - Volume & Mute

        /// <summary>
        /// BGM 볼륨(0~1)을 설정합니다.
        /// </summary>
        public void SetBgmVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            // 즉시 반영: 활성/비활성 소스 모두 스케일 반영
            if (activeBgm != null)
            {
                // 활성 BGM은 현재 상대볼륨을 유지해야 하므로, 절대값으로 보정하지 않고 스케일링 대상에서 사용
                // 여기서는 즉시 절대 설정 대신, 페이드 로직에서 bgmVolume을 곱하는 구조를 유지
                activeBgm.volume = Mathf.Clamp01(activeBgm.volume); // 범위 안전
            }
            if (inactiveBgm != null)
            {
                inactiveBgm.volume = Mathf.Clamp01(inactiveBgm.volume);
            }
        }

        /// <summary>
        /// SFX 볼륨(0~1)을 설정합니다.
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
        }

        /// <summary>
        /// 전체 오디오 뮤트 여부를 설정합니다.
        /// </summary>
        public void MuteAll(bool mute)
        {
            muted = mute;
            ApplyMute();
        }

        /// <summary>
        /// 현재 재생 중인 BGM 소스들의 절대 볼륨을 즉시 설정합니다.
        /// bgmVolume 스케일과 무관하게 즉시 반영됩니다.
        /// </summary>
        public void SetCurrentBgmVolume(float volume)
        {
            float v = Mathf.Clamp01(volume);
            if (activeBgm != null) activeBgm.volume = v;
            if (inactiveBgm != null) inactiveBgm.volume = Mathf.Min(inactiveBgm.volume, v);
        }

        /// <summary>
        /// 현재 재생 중인 BGM 절대 볼륨을 반환합니다. 재생 중이 아니면 0 반환.
        /// </summary>
        public float GetCurrentBgmVolume()
        {
            if (activeBgm != null && activeBgm.isPlaying)
                return Mathf.Clamp01(activeBgm.volume);
            return 0f;
        }

        #endregion

        #region Public API - SFX

        /// <summary>
        /// 지정한 AudioClip으로 SFX를 재생합니다.
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="volume">로컬 볼륨 계수(0~1), 최종은 SFX 볼륨과 곱연산</param>
        /// <param name="pitch">피치(기본 1)</param>
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("SFX 재생 실패: AudioClip이 비어 있습니다.");
                return;
            }

            var src = GetFreeSfxSource();
            if (src == null)
            {
                Debug.LogWarning("SFX 재생 실패: 사용 가능한 오디오 소스가 없습니다.");
                return;
            }

            src.pitch = pitch;
            src.volume = sfxVolume * Mathf.Clamp01(volume);
            src.PlayOneShot(clip);
        }

        /// <summary>
        /// AudioLibrary에 등록된 키로 SFX를 재생합니다.
        /// </summary>
        /// <param name="clipName">라이브러리 키</param>
        /// <param name="volume">로컬 볼륨 계수(0~1), 최종은 SFX 볼륨과 곱연산</param>
        /// <param name="pitch">피치(기본 1)</param>
        public void PlaySFX(string clipName, float volume = 1f, float pitch = 1f)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                Debug.LogWarning("SFX 재생 실패: 클립 이름이 비어 있습니다.");
                return;
            }

            var clip = LoadClipByName(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"SFX 재생 실패: 오디오 라이브러리에서 '{clipName}' 키를 찾지 못했습니다.");
                return;
            }

            PlaySFX(clip, volume, pitch);
        }

        /// <summary>
        /// 사용 가능한 SFX 소스를 얻거나, 필요 시 자동 확장해 반환합니다.
        /// </summary>
        private AudioSource GetFreeSfxSource()
        {
            foreach (var s in sfxPool)
            {
                if (!s.isPlaying)
                    return s;
            }

            if (sfxAutoExpand)
            {
                var src = transform.Find("SFX_Pool").gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.volume = sfxVolume;
                sfxPool.Add(src);
                return src;
            }

            return null;
        }

        #endregion

        #region Public API - BGM

        /// <summary>
        /// 지정 키의 BGM이 이미 재생 중이면 유지하고, 다르면 전환합니다.
        /// </summary>
        public void PlayBGMIfChanged(string clipName, bool loop = true, float fade = 0.5f)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                Debug.LogWarning("BGM 재생 실패: 클립 이름이 비어 있습니다.");
                return;
            }

            var clip = LoadClipByName(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"BGM 재생 실패: 오디오 라이브러리에서 '{clipName}' 키를 찾지 못했습니다.");
                return;
            }

            if (activeBgm != null && activeBgm.isPlaying && activeBgm.clip == clip)
            {
                activeBgm.loop = loop; // 설정 동기화만
                return;
            }

            PlayBGM(clip, loop, fade);
        }

        /// <summary>
        /// 지정 클립이 이미 재생 중이면 유지하고, 다르면 전환합니다.
        /// </summary>
        public void PlayBGMIfChanged(AudioClip clip, bool loop = true, float fade = 0.5f)
        {
            if (clip == null)
            {
                Debug.LogWarning("BGM 재생 실패: AudioClip이 비어 있습니다.");
                return;
            }

            if (activeBgm != null && activeBgm.isPlaying && activeBgm.clip == clip)
            {
                activeBgm.loop = loop;
                return;
            }

            PlayBGM(clip, loop, fade);
        }

        /// <summary>
        /// 지정한 AudioClip으로 BGM을 재생/전환합니다.
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="loop">루프 여부</param>
        /// <param name="fade">크로스페이드 시간(초)</param>
        public void PlayBGM(AudioClip clip, bool loop = true, float fade = 0.5f)
        {
            if (clip == null)
            {
                Debug.LogWarning("BGM 재생 실패: AudioClip이 비어 있습니다.");
                return;
            }

            CancelCurrentBgmTask();
            bgmCts = new CancellationTokenSource();
            _ = PlayBgmInternalAsync(clip, loop, fade, bgmCts.Token);
        }

        /// <summary>
        /// AudioLibrary에 등록된 키로 BGM을 재생/전환합니다.
        /// </summary>
        /// <param name="clipName">라이브러리 키</param>
        /// <param name="loop">루프 여부</param>
        /// <param name="fade">크로스페이드 시간(초)</param>
        public void PlayBGM(string clipName, bool loop = true, float fade = 0.5f)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                Debug.LogWarning("BGM 재생 실패: 클립 이름이 비어 있습니다.");
                return;
            }

            var clip = LoadClipByName(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"BGM 재생 실패: 오디오 라이브러리에서 '{clipName}' 키를 찾지 못했습니다.");
                return;
            }

            PlayBGM(clip, loop, fade);
        }

        /// <summary>
        /// 현재 재생 중인 BGM을 페이드아웃하여 정지합니다.
        /// </summary>
        /// <param name="fade">페이드아웃 시간(초)</param>
        public void StopBGM(float fade = 0.3f)
        {
            CancelCurrentBgmTask();
            bgmCts = new CancellationTokenSource();
            _ = StopBgmInternalAsync(fade, bgmCts.Token);
        }

        /// <summary>
        /// 내부용 BGM 전환 처리(크로스페이드).
        /// </summary>
        private async UniTask PlayBgmInternalAsync(AudioClip clip, bool loop, float fade, CancellationToken ct)
        {
            // 비활성 소스에 새 클립 로드
            inactiveBgm.clip = clip;
            inactiveBgm.loop = loop;
            inactiveBgm.volume = 0f;
            inactiveBgm.Play();

            if (fade <= 0f)
            {
                // 페이드 없이 전환
                activeBgm.Stop();
                inactiveBgm.volume = bgmVolume;
                SwapBgmSources();
                return;
            }

            float time = 0f;
            float duration = Mathf.Max(0.01f, fade);
            float startActive = activeBgm.isPlaying ? activeBgm.volume : 0f;

            while (time < duration)
            {
                if (ct.IsCancellationRequested)
                    return;

                time += Time.unscaledDeltaTime; // BGM 전환은 보통 게임 일시정지에도 진행되도록 unscaled 사용
                float t = Mathf.Clamp01(time / duration);

                inactiveBgm.volume = Mathf.Lerp(0f, bgmVolume, t);
                activeBgm.volume = Mathf.Lerp(startActive, 0f, t);

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            activeBgm.Stop();
            inactiveBgm.volume = bgmVolume;
            SwapBgmSources();
        }

        /// <summary>
        /// 내부용 BGM 정지 처리(페이드아웃).
        /// </summary>
        private async UniTask StopBgmInternalAsync(float fade, CancellationToken ct)
        {
            if (!activeBgm.isPlaying)
                return;

            if (fade <= 0f)
            {
                activeBgm.Stop();
                activeBgm.clip = null;
                return;
            }

            float time = 0f;
            float duration = Mathf.Max(0.01f, fade);
            float start = activeBgm.volume;

            while (time < duration)
            {
                if (ct.IsCancellationRequested)
                    return;

                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                activeBgm.volume = Mathf.Lerp(start, 0f, t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            activeBgm.Stop();
            activeBgm.clip = null;
        }

        /// <summary>
        /// 활성/비활성 BGM 소스를 교체합니다.
        /// </summary>
        private void SwapBgmSources()
        {
            var tmp = activeBgm;
            activeBgm = inactiveBgm;
            inactiveBgm = tmp;
        }

        /// <summary>
        /// 진행 중인 BGM 관련 비동기 작업을 취소합니다.
        /// </summary>
        private void CancelCurrentBgmTask()
        {
            if (bgmCts != null)
            {
                bgmCts.Cancel();
                bgmCts.Dispose();
                bgmCts = null;
            }
        }

        #endregion

        #region Lookup From Library

        /// <summary>
        /// 라이브러리에서 키로 AudioClip을 조회합니다.
        /// </summary>
        private AudioClip LoadClipByName(string clipName)
        {
            if (audioLibrary != null && audioLibrary.TryGet(clipName, out var found))
                return found;
            return null;
        }

        #endregion
    }
