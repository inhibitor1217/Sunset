using UnityEngine;

public class InputManager : MonoBehaviour
{

    public MainImage mainImage;
    public RectTransform containerRectTransform;
    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (containerRectTransform
                && RectTransformUtility.RectangleContainsScreenPoint(
                        containerRectTransform, touch.position
                    ))
            {
                if (mainImage)
                    mainImage.UpdatePosition(touch.deltaPosition);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (containerRectTransform
                && RectTransformUtility.RectangleContainsScreenPoint(
                    containerRectTransform, touch0.position
                )
                && RectTransformUtility.RectangleContainsScreenPoint(
                    containerRectTransform, touch1.position
                ))
            {
                Vector2 touch0Prev = touch0.position - touch0.deltaPosition;
                Vector2 touch1Prev = touch1.position - touch1.deltaPosition;

                float prevMagnitude = (touch1Prev - touch0Prev).magnitude;
                float currMagnitude = (touch1.position - touch0.position).magnitude;

                if (mainImage)
                    mainImage.UpdateScale(currMagnitude / prevMagnitude);
            }
        }
#else
        if (mainImage)
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");
            mainImage.UpdatePosition(-15f * new Vector2(inputX, inputY));

            if (Input.GetKey(KeyCode.Q))
                mainImage.UpdateScale(1.1f);
            else if (Input.GetKey(KeyCode.W))
                mainImage.UpdateScale(0.9f);
        }
#endif
    }

}
