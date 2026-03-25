export interface NotificationPrefs {
  pushNewFollower: boolean;
  pushLikes: boolean;
  pushReplies: boolean;
  pushMentions: boolean;
  pushReposts: boolean;
  pushDirectMessages: boolean;
  emailWeeklyDigest: boolean;
  emailSecurityAlerts: boolean;
  emailProductUpdates: boolean;
  quietHoursEnabled: boolean;
  quietHoursFrom: string | null;
  quietHoursUntil: string | null;
}

export interface PrivacySettings {
  accountVisibility: string;
  whoCanMessage: string;
  whoCanSeeFollowers: string;
  allowTagging: boolean;
  allowReposts: boolean;
  showInSearch: boolean;
  showActivityStatus: boolean;
  personalisedRecommendations: boolean;
  shareDataWithPartners: boolean;
}

export interface FeedPrefs {
  feedOrder: string;
  showSuggestedPosts: boolean;
  showTrendingTopics: boolean;
  sensitiveContent: string;
  autoplayVideos: string;
  showLikedPostsInOtherFeeds: boolean;
  mutedWords: string[];
}

export interface SessionInfo {
  id: string;
  createdAt: string;
  expiresAt: string;
  isCurrent: boolean;
}
