import './ChatBubble.css'
import { CURRENT_USER, chatController } from '../../Constants'
import DeleteIcon from '@mui/icons-material/Delete';
import { Button } from '@mui/material';
export default function ChatBubble({message})
{

    async function handleDelete(ev)
    {
        const request = await fetch(chatController + `/DeleteMessage/${message.messageId}`,{method:'DELETE'})
        if (request.ok)
        {
            location.reload()
        }
        else
        {
            alert('uh oh')
            }
    }
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
            <Button onClick={handleDelete}><DeleteIcon/></Button>
        {message.content}
    </div>
        </div>
}