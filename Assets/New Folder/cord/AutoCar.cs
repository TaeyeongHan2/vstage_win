using UnityEngine;

public class AutoCar : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float flySpeed = 2f;
    private Vector3 targetPosition;

    void Start()
    {
        // 처음 목표 위치 설정
        SetRandomTarget();
    }

    void Update()
    {
        // 목표 위치까지 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 날아다니는 효과(Y축 이동)
        // (예시: 위아래로 살짝 오르내리기)
        float yOffset = Mathf.Sin(Time.time) * flySpeed;
        transform.position = new Vector3(transform.position.x, yOffset, transform.position.z);

        // 목표 위치에 도달하면 새로운 목표 위치 설정
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            SetRandomTarget();
        }
    }

    void SetRandomTarget()
    {
        // 무대 크기(예: x, z 범위 -10~10, y는 필요 없음)
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        targetPosition = new Vector3(x, 0, z);
    }
}
