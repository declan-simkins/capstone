using UnityEngine;

namespace Camera_Behaviours
{
	public class Follow_Camera : MonoBehaviour
	{
		#region Serialized Fields
		[SerializeField] private GameObject follow_cam_obj;
		[SerializeField] private GameObject target;
		
		[Tooltip("A box surrounding what should be the visible the play space.")]
		[SerializeField] private BoxCollider2D bounding_box;
		#endregion Serialized Fields

		
		#region Private Fields
		private Camera follow_cam;
		#endregion Private Fields


		public void Set_Target(GameObject new_target)
		{
			this.target = new_target;
		}
		
		
		#region Private Methods
		private void Awake()
		{
			this.follow_cam = this.follow_cam_obj.GetComponent<Camera>();
		}

		private void LateUpdate()
		{
			this.Follow_Target();
			this.Stop_On_Bounding_Box();
		}

		private void Follow_Target()
		{
			if (this.target is null) return;
			Vector3 target_pos = this.target.transform.position;
			Vector3 camera_pos = this.follow_cam_obj.transform.position;
		
			Vector3 new_pos = new Vector3(
				target_pos.x
				, target_pos.y
				, camera_pos.z
			);
			this.follow_cam_obj.transform.position = new_pos;
		}

		private void Stop_On_Bounding_Box()
		{
			Vector3 box_center = this.bounding_box.transform.TransformVector(this.bounding_box.offset);
			Vector3 box_size = this.bounding_box.size;
			float scale_x = this.bounding_box.transform.localScale.x;
			float scale_y = this.bounding_box.transform.localScale.y;
			
			float box_right_edge = box_center.x + box_size.x * scale_x / 2;
			float box_left_edge = box_center.x + -box_size.x * scale_x / 2;
			float box_top_edge = box_center.y + box_size.y  * scale_y / 2;
			float box_bottom_edge = box_center.y + -box_size.y * scale_y / 2;
		
			Vector3 camera_pos = this.follow_cam_obj.transform.position;
			float camera_aspect_ratio = this.follow_cam.aspect;
			float camera_size = this.follow_cam.orthographicSize;
			float camera_top_edge = camera_pos.y + camera_size;
			float camera_bottom_edge = camera_pos.y + -camera_size;
			float camera_right_edge = camera_pos.x + camera_size * camera_aspect_ratio;
			float camera_left_edge = camera_pos.x + -camera_size * camera_aspect_ratio;

			Vector3 new_pos = new Vector3(camera_pos.x, camera_pos.y, camera_pos.z);
			if (camera_right_edge > box_right_edge) {
				new_pos.x = box_right_edge - (camera_right_edge - camera_pos.x);
			}
			else if (camera_left_edge < box_left_edge) {
				new_pos.x = box_left_edge - (camera_left_edge - camera_pos.x);
			}

			if (camera_top_edge > box_top_edge) {
				new_pos.y = box_top_edge - (camera_top_edge - camera_pos.y);
			}
			else if (camera_bottom_edge < box_bottom_edge) {
				new_pos.y = box_bottom_edge - (camera_bottom_edge - camera_pos.y);
			}

			this.follow_cam_obj.transform.position = new_pos;
		}
		#endregion Private Methods
	}
}
