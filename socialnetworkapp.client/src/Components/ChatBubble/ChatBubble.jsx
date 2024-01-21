import './ChatBubble.css'
import { CURRENT_USER } from '../../Constants'

export default function ChatBubble({message})
{
    const user = JSON.parse(localStorage.getItem(CURRENT_USER))
    const direction = message.senderId !== user.userId
        return <div className="chatbubble-container"
        style={{
        flexDirection:direction?'row':'row-reverse'
    }}>
        <div className="chatbubble" style={
        {
            color: direction ? 'white' : 'black',
            backgroundColor: direction ? 'var(--sky)' : 'var(--soft-grey)'
        }
    }>
        {message.content}
    </div>
        </div>
}