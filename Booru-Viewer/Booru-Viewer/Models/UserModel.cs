using System;

namespace Booru_Viewer.Models
{
	public class UserModel
	{
		// ReSharper disable InconsistentNaming
			public int id { get; set; }
			public DateTime created_at { get; set; }
			public string name { get; set; }
			public int inviter_id { get; set; }
			public int level { get; set; }
			public int base_upload_limit { get; set; }
			public int post_upload_count { get; set; }
			public int post_update_count { get; set; }
			public int note_update_count { get; set; }
			public bool is_banned { get; set; }
			public bool can_approve_posts { get; set; }
			public bool can_upload_free { get; set; }
			public bool is_super_voter { get; set; }
			public string level_string { get; set; }
			public bool has_mail { get; set; }
			public bool receive_email_notifications { get; set; }
			public bool always_resize_images { get; set; }
			public bool enable_post_navigation { get; set; }
			public bool new_post_navigation_layout { get; set; }
			public bool enable_privacy_mode { get; set; }
			public bool enable_sequential_post_navigation { get; set; }
			public bool hide_deleted_posts { get; set; }
			public bool style_usernames { get; set; }
			public bool enable_auto_complete { get; set; }
			public bool show_deleted_children { get; set; }
			public bool has_saved_searches { get; set; }
			public bool disable_categorized_saved_searches { get; set; }
			public bool disable_tagged_filenames { get; set; }
			public bool enable_recent_searches { get; set; }
			public bool disable_cropped_thumbnails { get; set; }
			public bool disable_mobile_gestures { get; set; }
			public bool enable_safe_mode { get; set; }
			public bool disable_responsive_mode { get; set; }
			public DateTime updated_at { get; set; }
			public string email { get; set; }
			public DateTime last_logged_in_at { get; set; }
			public DateTime last_forum_read_at { get; set; }
			public string recent_tags { get; set; }
			public int comment_threshold { get; set; }
			public string default_image_size { get; set; }
			public string favorite_tags { get; set; }
			public string blacklisted_tags { get; set; }
			public string time_zone { get; set; }
			public int per_page { get; set; }
			public string custom_style { get; set; }
			public int favorite_count { get; set; }
			public int api_regen_multiplier { get; set; }
			public int api_burst_limit { get; set; }
			public float remaining_api_limit { get; set; }
			public int statement_timeout { get; set; }
			public int favorite_group_limit { get; set; }
			public object favorite_limit { get; set; }
			public int tag_query_limit { get; set; }
			public bool can_comment_vote { get; set; }
			public bool can_remove_from_pools { get; set; }
			public bool is_comment_limited { get; set; }
			public bool can_comment { get; set; }
			public bool can_upload { get; set; }
			public int max_saved_searches { get; set; }

	}
}
