export class User {
  constructor({
    id,
    username,
    profileImageUrl
  }) {
    this.id = id;
    this.username = username;
    this.profileImageUrl = profileImageUrl;
  }

  static fromDbRow(row) {
    return new User({
      id: row.Id || row.UserId,
      username: row.Username,
      profileImageUrl: row.ProfileImageUrl
    });
  }

  toJSON() {
    return {
      id: this.id,
      username: this.username,
      profileImageUrl: this.profileImageUrl
    };
  }
}