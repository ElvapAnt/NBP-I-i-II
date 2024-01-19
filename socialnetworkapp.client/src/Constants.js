

export const rootUrl = 'http://localhost:5114'
export const userController = rootUrl + '/User'
export const chatController = rootUrl + '/Chat'
export const notificationController = rootUrl + '/Notification'
export const postController = rootUrl + '/Post'
export const CURRENT_USER = "currentUser"
export const CURRENT_CHAT = 'currentChat'
export const TALKING_TO = 'talkingTo'

export const mod = (n,m) => {
    return ((n % m) + m) % m;
}

