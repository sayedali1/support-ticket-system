export interface Comment {
  id: number;
  content: string;
  authorName: string;
  createdAt: string;
}

export interface CommentCreateRequest {
  content: string;
}

export interface TimelineEntry {
  type: 'Comment' | 'ActivityLog';
  timestamp: string;
  authorName: string;
  content?: string;
  fieldChanged?: string;
  oldValue?: string;
  newValue?: string;
}