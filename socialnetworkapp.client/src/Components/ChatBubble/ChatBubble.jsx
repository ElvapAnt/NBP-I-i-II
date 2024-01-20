import './ChatBubble.css'
import { CURRENT_USER } from '../../Constants'

export default function ChatBubble({message})
{
    const direction = message.recipient === JSON.parse(localStorage.getItem(CURRENT_USER)).username
   /* const file = message.file*/
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