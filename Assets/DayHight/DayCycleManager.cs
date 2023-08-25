using System;
using UnityEngine;

[ExecuteInEditMode]
public class DayCycleManager : MonoBehaviour
{
    [Range(0f, 1f)]
    public float TimeOfDay; // Время дня от 0 до 1
    public float DayDuration = 30f; // Продолжительность дня в секундах

    public Gradient NightGradient; // Градиент цвета для ночи
    public Gradient MorningGradient; // Градиент цвета для утра

    public AnimationCurve SunCurve; // Кривая анимации солнца
    public AnimationCurve MoonCurve; // Кривая анимации луны
    public AnimationCurve SkyboxCurve; // Кривая анимации неба

    public Material DaySkybox; // Материал дневного неба
    public Material NightSkybox; // Материал ночного неба

    public ParticleSystem Stars; // Система частиц для звезд

    public Light Sun; // Источник света - солнце

    public float sunIntensity; // Интенсивность света солнца

    [Range(0.01f, 0.5f)]
    public float moonTeleportThreshold = 0.05f; // Порог телепортации луны между позициями

    private Vector3 defaultAngles; // Углы по умолчанию для света

    private void Start() // В начале (при запуске)
    {
        defaultAngles = Sun.transform.localEulerAngles; // Сохранить углы по умолчанию для света
    }

    private void Update() // В каждом кадре (каждое обновление)
    {
        if (Application.isPlaying) // Если приложение находится в режиме проигрывания
        {
            TimeOfDay += Time.deltaTime / DayDuration; // Увеличить время дня в зависимости от времени и продолжительности дня
        }
        if (TimeOfDay >= 1) TimeOfDay -= 1; // Если время дня больше или равно 1, отнять 1

        RenderSettings.skybox.Lerp(NightSkybox, DaySkybox, SkyboxCurve.Evaluate(TimeOfDay)); // Лерп между ночным и дневным небом в зависимости от времени дня
        RenderSettings.sun = Sun; // Установить источник света в настройках прорисовки
        DynamicGI.UpdateEnvironment(); // Обновить глобальное освещение

        if (TimeOfDay >= 0.51f && TimeOfDay <= 1) // Если время дня находится между 0.51 и 1
        {
            if (!Stars.isEmitting) // Проверяем включены ли звезды
            {
                Stars.Play(); // Запустить систему звезд
            }
        }
        else
        {
            if (Stars.isEmitting) // Если звезды включены 
            {
                Stars.Stop(); // Остановить систему звезд
            }
        }

        float rotationProgress = TimeOfDay * 360f; // Прогресс вращения в зависимости от времени дня
        float sunIntensityMultiplier = SunCurve.Evaluate(TimeOfDay); // Множитель интенсивности солнца
        float moonIntensityMultiplier = MoonCurve.Evaluate(TimeOfDay); // Множитель интенсивности луны

        float finalIntensity = sunIntensity * (sunIntensityMultiplier + moonIntensityMultiplier); // Общая интенсивность света
        Sun.intensity = Mathf.SmoothStep(sunIntensity * sunIntensityMultiplier, sunIntensity * moonIntensityMultiplier + 2, moonIntensityMultiplier); // Интенсивность света с интерполяцией

        if (TimeOfDay < 0.5f) // Если время дня меньше 0.5
        {
            Sun.color = MorningGradient.Evaluate(TimeOfDay * 2); // Установка цвета для утра в зависимости от времени дня
        }
        else
        {
            Sun.color = NightGradient.Evaluate((TimeOfDay - 0.5f) * 2); // Установка цвета для ночи
        }

        Quaternion sunRotation = Quaternion.Euler(rotationProgress, 180, 0); // Вращение солнца
        Quaternion moonRotation = Quaternion.Euler(rotationProgress + 180f, 180, 0); // Вращение луны

        if (moonIntensityMultiplier > moonTeleportThreshold) // Если множитель интенсивности луны больше порога телепортации
        {
            Sun.transform.localRotation = moonRotation; // Установить вращение луны
        }
        else
        {
            Sun.transform.localRotation = sunRotation; // Установить вращение солнца
        }
    }
}