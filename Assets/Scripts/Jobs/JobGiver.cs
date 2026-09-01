using UnityEngine;

namespace OpenWorld.Jobs
{
    public class JobGiver : MonoBehaviour
    {
        public JobType jobType = JobType.Taxi;
        public float interactDistance = 4f;
        public Transform player;

        void Update()
        {
            if (player == null)
            {
                var p = FindObjectOfType<PlayerController>();
                if (p != null) player = p.transform;
                else return;
            }

            float d = Vector3.Distance(player.position, transform.position);
            if (d > interactDistance) return;

            bool hasJob = JobManager.Instance != null && JobManager.Instance.HasJob;
            string hint = hasJob ? "" : "E — взять работу: " + JobName(jobType);
            if (!hasJob && Input.GetKeyDown(KeyCode.E))
            {
                JobManager.Instance.StartJob(jobType);
            }

            if (GameManager.Instance != null && hint != "" && d < interactDistance)
            {
                var car = player.GetComponent<CarInteraction>();
                bool inCar = car != null && car.CurrentCar != null;
                if (!inCar && !hasJob) GameManager.Instance.Hint = hint;
            }
        }

        static string JobName(JobType t)
        {
            switch (t)
            {
                case JobType.Taxi: return "Такси";
                case JobType.Courier: return "Курьер";
                case JobType.Collect: return "Сбор";
                default: return t.ToString();
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactDistance);
        }
    }
}
