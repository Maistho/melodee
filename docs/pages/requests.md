---
title: Requests
permalink: /requests/
---

# Requests

Requests in Melodee allow users to ask for new music to be added to the library or report issues with existing content. It's a collaborative way for users to communicate their music needs to administrators or other users who manage the library.

## What Are Requests?

Requests are user-submitted tickets that can be used to:

- **Request new albums** to be added to the library
- **Request specific songs** that are missing
- **Report artist corrections** (misspellings, wrong metadata)
- **Report album corrections** (wrong cover art, incorrect track listing)
- **General requests** for other library-related needs

Requests support comments, activity tracking, and automatic completion when the requested content is added.

## Request Categories

| Category | Description | Use Case |
|----------|-------------|----------|
| **Add Album** | Request a new album | "Please add 'Abbey Road' by The Beatles" |
| **Add Song** | Request a specific song | "I need 'Bohemian Rhapsody' by Queen" |
| **Artist Correction** | Fix artist information | "Artist name is misspelled as 'Beetles'" |
| **Album Correction** | Fix album information | "Wrong cover art on 'Dark Side of the Moon'" |
| **General** | Other requests | "Can we add more jazz albums?" |

## Request Status

Requests progress through these statuses:

| Status | Description |
|--------|-------------|
| **Pending** | New request, awaiting action |
| **In Progress** | Someone is working on fulfilling the request |
| **Completed** | Request has been fulfilled |
| **Rejected** | Request was denied (duplicate, unavailable, etc.) |

## Creating Requests

### From the Melodee UI

1. Navigate to **Requests** in the menu
2. Click **New Request**
3. Select a category (Add Album, Add Song, etc.)
4. Fill in the details:
   - **Description**: Explain what you're requesting
   - **Artist Name**: The artist (for album/song requests)
   - **Album Title**: The album name (for album requests)
   - **Song Title**: The song name (for song requests)
   - **Release Year**: Optional year to help identify the correct version
   - **External URL**: Link to Discogs, MusicBrainz, or streaming service
   - **Notes**: Additional information
5. Submit the request

### Best Practices for Requests

- **Be specific**: Include artist name, album title, and year when possible
- **Add links**: External URLs to Discogs, MusicBrainz, or Spotify help identify the exact release
- **Check first**: Search the library to make sure the content isn't already there
- **One request per item**: Create separate requests for different albums/songs

## Request Details

Each request contains:

| Field | Description |
|-------|-------------|
| **Category** | Type of request (Add Album, Add Song, etc.) |
| **Status** | Current state (Pending, In Progress, Completed, Rejected) |
| **Description** | User's explanation of what they need |
| **Artist Name** | Artist associated with the request |
| **Album Title** | Album name (for album-related requests) |
| **Song Title** | Song name (for song requests) |
| **Release Year** | Year to help identify correct version |
| **External URL** | Link to external reference (Discogs, MusicBrainz) |
| **Notes** | Additional information |
| **Created By** | User who submitted the request |
| **Created At** | When the request was submitted |
| **Last Activity** | Most recent activity on the request |

## Comments and Discussion

Requests support threaded comments for discussion:

- **User comments**: Users can ask questions or provide updates
- **System comments**: Automatic messages when status changes or content is added
- **Replies**: Comments can be nested for threaded discussions

### Comment Features

- Markdown formatting support
- Timestamps showing when comments were posted
- User attribution for each comment
- System messages for automatic updates

## Automatic Completion

One of the most powerful features of Melodee's request system is **automatic completion**. When you request an album or song, Melodee monitors new content being added to the library. If matching content is found:

1. The request is automatically marked as **Completed**
2. A system comment is added with a link to the new content
3. The requester is notified (if notifications are enabled)

### How Matching Works

For **Add Album** requests:
- Melodee compares the requested artist name and album title
- Normalized text matching handles minor spelling differences
- Release year is checked if specified in the request
- If a match is found when the album is added, the request completes

For **Add Song** requests:
- Similar matching on artist name and song title
- Completes when a matching song is added to any album

## Managing Requests

### For Users

- **View your requests**: See all requests you've submitted
- **Edit requests**: Update pending requests with more information
- **Complete requests**: Mark your own requests as completed
- **Delete requests**: Remove pending requests you no longer need
- **Comment**: Add updates or respond to questions

### For Administrators

- **View all requests**: See requests from all users
- **Change status**: Move requests to In Progress, Completed, or Rejected
- **Add comments**: Provide updates on request progress
- **Prioritize**: Focus on frequently requested content

## Request Workflow

### Typical Flow

```
User creates request (Pending)
       ↓
Admin sees request and starts working (In Progress)
       ↓
Content is added to library
       ↓
Request auto-completes OR admin marks complete (Completed)
       ↓
System comment notifies user with link to content
```

### Alternative Flows

**Duplicate Request:**
```
User creates request → Admin marks as Rejected → Comment explains duplicate
```

**Unavailable Content:**
```
User creates request → Admin marks as Rejected → Comment explains unavailability
```

**Self-Fulfilled:**
```
User creates request → User finds content elsewhere → User marks as Completed
```

## Activity Tracking

Every request tracks activity:

| Activity Type | Description |
|---------------|-------------|
| **User Comment** | Someone added a comment |
| **System Comment** | Automatic system message |
| **Status Changed** | Request status was updated |
| **Edited** | Request details were modified |

The **Last Activity** field shows the most recent action, helping users and admins identify requests that need attention.

## Participants

Melodee tracks who participates in each request:

- **Creator**: The user who submitted the request
- **Commenters**: Users who have commented on the request

This helps identify who to notify when there are updates.

## Search and Filtering

Find requests using:

- **Status filter**: Show only Pending, In Progress, etc.
- **Category filter**: Show only Add Album requests, etc.
- **My requests**: Show only requests you created
- **Text search**: Search descriptions, artist names, album titles

## Use Cases

### Building a Shared Library

In a household or organization:
1. Users submit requests for music they want
2. The library manager reviews requests
3. High-priority or frequently requested content is added first
4. Requests auto-complete as content is added

### Music Discovery Queue

Use requests as a "to-buy" or "to-add" list:
1. Create requests when you discover interesting albums
2. Add external links for reference
3. Work through the list over time
4. Track what's been added vs. what's still needed

### Quality Control

Report issues with existing content:
1. Create an Artist Correction or Album Correction request
2. Describe the problem in detail
3. Admin fixes the issue
4. Request is marked complete

## API Access

Requests are accessible via the Melodee API:

```
# List requests
GET /api/v1/requests

# Get request by API key
GET /api/v1/requests/{apiKey}

# Create a new request
POST /api/v1/requests
Body: {
  "category": "AddAlbum|AddSong|ArtistCorrection|AlbumCorrection|General",
  "description": "Please add this album",
  "artistName": "The Beatles",
  "albumTitle": "Abbey Road",
  "releaseYear": 1969,
  "externalUrl": "https://www.discogs.com/...",
  "notes": "Original UK pressing preferred"
}

# Update a request
PUT /api/v1/requests/{apiKey}

# Complete a request
POST /api/v1/requests/{apiKey}/complete

# Delete a request
DELETE /api/v1/requests/{apiKey}

# Add a comment
POST /api/v1/requests/{apiKey}/comments
Body: {
  "body": "Any update on this?",
  "parentCommentId": null
}
```

## Tips for Effective Requests

1. **Search first**: Check if the content already exists
2. **Be specific**: Include all relevant details (artist, album, year)
3. **Add references**: External URLs help identify exact releases
4. **One item per request**: Makes tracking and completion easier
5. **Check back**: Monitor your requests for comments and updates
6. **Mark complete**: If you found the content elsewhere, mark it done

## Privacy

- Requests are visible to all users by default
- Comments show the username of the commenter
- Activity is tracked for transparency
- Admins can see all requests; users see their own and public requests

---

Have questions about requests? Open an issue on GitHub or check the [API documentation](/api/) for technical details.
