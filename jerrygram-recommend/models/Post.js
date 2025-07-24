export class Post {
  constructor({
    id,
    caption,
    imageUrl,
    createdAt,
    likes = 0,
    liked = false,
    user,
    score = 0
  }) {
    this.id = id;
    this.caption = caption;
    this.imageUrl = imageUrl;
    this.createdAt = createdAt;
    this.likes = likes;
    this.liked = liked;
    this.user = user;
    this.score = score;
  }

  static fromDbRow(row) {
    return new Post({
      id: row.Id,
      caption: row.Caption,
      imageUrl: row.ImageUrl,
      createdAt: row.CreatedAt,
      likes: parseInt(row.Likes, 10),
      liked: false,
      user: {
        id: row.UserId,
        username: row.Username,
        profileImageUrl: row.ProfileImageUrl
      }
    });
  }

  toJSON() {
    return {
      id: this.id,
      caption: this.caption,
      imageUrl: this.imageUrl,
      createdAt: this.createdAt,
      likes: this.likes,
      liked: this.liked,
      user: this.user,
      ...(this.score > 0 && { score: this.score })
    };
  }
}