//
//  MegacoolFFI.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#ifndef MegacoolFFI_h
#define MegacoolFFI_h

#import <stdbool.h>
#import <stddef.h>

#if __STDC_VERSION__ < 199901L
// Define no-op restrict qualifier for compilers that don't support it
#define restrict
#endif


// The layouts of these structs must not be changed without syncing with
// the definitions in the unity wrapper in Megacool.cs.

typedef enum mcl_share_state {
    MCL_SHARE_STATE_SENT = 0,
    MCL_SHARE_STATE_CLICKED,
    MCL_SHARE_STATE_OPENED,
    MCL_SHARE_STATE_INSTALLED,
} mcl_share_state;


typedef enum mcl_capture_method {
    MCL_CAPTURE_METHOD_VIEW = 0,
    MCL_CAPTURE_METHOD_OPENGLES2,
    MCL_CAPTURE_METHOD_OPENGLES3,
    MCL_CAPTURE_METHOD_METAL,
} mcl_capture_method;


typedef enum mcl_gif_color_table {
    MCL_GIF_COLOR_TABLE_DYNAMIC = 0,
    MCL_GIF_COLOR_TABLE_FIXED,
    MCL_GIF_COLOR_TABLE_ANALYZE_FIRST,
} mcl_gif_color_table;


typedef enum mcl_frame_origin {
    MCL_UPPER_LEFT = 0,
    MCL_BOTTOM_LEFT = 1,
} mcl_frame_origin;

typedef enum mcl_popover_arrow_direction {
    MCL_POPOVER_ARROW_DIRECTION_UNSET = -1,
    MCL_POPOVER_ARROW_DIRECTION_UP = 1 << 0,
    MCL_POPOVER_ARROW_DIRECTION_DOWN = 1 << 1,
    MCL_POPOVER_ARROW_DIRECTION_LEFT = 1 << 2,
    MCL_POPOVER_ARROW_DIRECTION_RIGHT = 1 << 3,
    MCL_POPOVER_ARROW_DIRECTION_ANY = MCL_POPOVER_ARROW_DIRECTION_UP | MCL_POPOVER_ARROW_DIRECTION_DOWN | MCL_POPOVER_ARROW_DIRECTION_LEFT | MCL_POPOVER_ARROW_DIRECTION_RIGHT,
} mcl_popover_arrow_direction;


typedef struct mcl_referral_code {
    const char *user_id;
    const char *share_id;
} mcl_referral_code;


typedef struct mcl_share {
    mcl_referral_code referral_code;
    mcl_share_state state;
    // Milliseconds since epoch
    double created_at;
    // Milliseconds since epoch
    double updated_at;
    // JSON-encoded arbitrary data attached to the share. Can be null.
    char *data_json;
    char *url;
} mcl_share;


typedef struct mcl_link_clicked_event {
    const char *url;
    const mcl_referral_code referral_code;
} mcl_link_clicked_event;


typedef struct mcl_received_share_opened_event {
    const char *sender_user_id;
    const bool is_first_session;
    const double created_at; // Milliseconds since epoch
    const mcl_share share;
} mcl_received_share_opened_event;


typedef struct mcl_sent_share_opened_event {
    const char *receiver_user_id;
    const bool is_first_session;
    const double created_at; // Milliseconds since epoch
    const mcl_share share;
} mcl_sent_share_opened_event;


static const int MCL_SHARE_STRATEGY_UNSET = -1;

// Set the mcl_rect.x to this if the source_rect is unset
static const float MCL_SHARE_POPOVER_SOURCE_RECT_UNSET = -1.0f;

typedef struct mcl_rect {
    float x;
    float y;
    float width;
    float height;
} mcl_rect;

typedef struct mcl_share_config {
    const char *recording_id;
    const char *message;
    const char *fallback_image;
    const char *url;
    const char *jsonData;
    int strategy;
    const mcl_rect popover_source_rect;
    const mcl_popover_arrow_direction popover_permitted_arrow_directions;
} mcl_share_config;

static const int MCL_RECORDING_OVERFLOW_STRATEGY_UNSET = -1;
static const int MCL_RECORDING_MAX_FRAMES_UNSET = -1;
static const int MCL_RECORDING_FRAME_RATE_UNSET = -1;
static const int MCL_RECORDING_PLAYBACK_FRAME_RATE_UNSET = -1;
static const int MCL_RECORDING_LAST_FRAME_DELAY_UNSET = -1;
static const double MCL_RECORDING_PEAK_LOCATION_UNSET = -1.0;

typedef struct mcl_recording_config {
    const char *recording_id;
    int overflow_strategy;
    int max_frames;
    int frame_rate;
    int playback_frame_rate;
    int last_frame_delay;
    double peak_location;
    const char *last_frame_overlay;
} mcl_recording_config;


typedef void (*MegacoolDidCompleteShareDelegate)(void);
typedef void (*MegacoolDidDismissShareViewDelegate)(void);
typedef void (*EventHandlerDelegate)(const void *, int);
typedef void (*OnLinkClickedEvent)(mcl_link_clicked_event);
typedef void (*OnReceivedShareOpenedEvent)(mcl_received_share_opened_event);
typedef void (*OnSentShareOpenedEvent)(mcl_sent_share_opened_event);
typedef void (*OnRetrievedShares)(mcl_share[], int);
typedef void (*OnUserIdReceivedDelegate)(const char *);


typedef struct mcl_megacool_config {
    const char *wrapper;
    const char *wrapper_version;
    const char *base_url;
    const OnLinkClickedEvent link_clicked_handler;
    const OnReceivedShareOpenedEvent received_share_opened_handler;
    const OnSentShareOpenedEvent sent_share_opened_handler;
    const MegacoolDidCompleteShareDelegate share_completed_handler;
    const MegacoolDidDismissShareViewDelegate share_dismissed_handler;
} mcl_megacool_config;

typedef struct mcl_preview_data mcl_preview_data;

typedef struct mcl_preview_frame {
    char *path;
    int delay_ms;
} mcl_preview_frame;


typedef struct mcl_frame_details {
    const unsigned char *pixels;
    int width;
    int height;
    int delay_ms;
} mcl_frame_details;


void mcl_start_with_app_config(const char *app_config, const mcl_megacool_config* config);


void mcl_manual_application_did_become_active(void);


#pragma mark - Recording and capture
void mcl_start_recording(const mcl_recording_config *config);

void mcl_register_score_change(int scoreDelta);

void mcl_capture_frame(const mcl_recording_config *config, bool force_add);

void mcl_pause_recording(void);

void mcl_stop_recording(void);

void mcl_delete_recording(const char *recording_id);

int mcl_get_number_of_frames(const char *recording_id);

int mcl_get_recording_score(const char *recording_id);

void mcl_get_user_id(OnUserIdReceivedDelegate callback);


#pragma mark - Shares
void mcl_present_share(const mcl_share_config *config);

void mcl_share_screenshot(const mcl_recording_config *recording_config, const mcl_share_config *share_config);

void mcl_present_share_to_messages(const mcl_share_config *config);

void mcl_present_share_to_mail(const mcl_share_config *config);

void mcl_delete_shares(bool (*filter)(mcl_share share));


#pragma mark - Configuration

void mcl_set_default_share_config(mcl_share_config *config);

void mcl_set_default_recording_config(mcl_recording_config *config);

void mcl_set_capture_method(mcl_capture_method captureMethod);

void mcl_set_capture_method_with_scale_factor(mcl_capture_method captureMethod, double scaleFactor);

void mcl_set_keep_completed_recordings(bool keep);

void mcl_set_gif_color_table(mcl_gif_color_table gifColorTable);


#pragma mark - Frame Enumeration API

mcl_preview_data *mcl_get_preview_for_recording(const char *recordingIdCString);

mcl_preview_frame *mcl_get_next_frame(mcl_preview_data *preview);

/**
 The pixel data in the returned frame needs to be copied by the caller if used across threads as the memory region is cached on the
 preview and will be re-used for future frames
 */
mcl_frame_details *mcl_get_next_frame_details(mcl_preview_data *preview, mcl_frame_origin origin);

mcl_frame_details *mcl_get_frame_details(mcl_preview_data *preview, mcl_preview_frame *frame,
                                         mcl_frame_origin origin);

int mcl_get_preview_frame_count(mcl_preview_data *preview);

void mcl_free_preview_data(mcl_preview_data *preview);

void mcl_free_preview_frame(mcl_preview_frame* frame);

void mcl_free_frame_details(mcl_frame_details *details);


#pragma mark - Debugging/development
void mcl_submit_debug_data_with_message(const char *message);

void mcl_reset_identity(void);

void mcl_set_debug_mode(bool debugMode);

bool mcl_get_debug_mode(void);

/* Helper for platforms that don't have easy pointer-to-pointer memcpy */
void mcl_memcpy(void *dst, const void *restrict src, size_t length);

#endif /* MegacoolFFI_h */
