import { Button } from '@mui/material'
import './ChatCard.css'
import { useNavigate } from 'react-router-dom'

export default function ChatCard({ username, chatItem, onChatSelect,thumbnail })
{
    const navigate = useNavigate()

    const handleClick = () => {
        onChatSelect(chatItem); // Call the function passed via props
        if (chatItem != null)
            navigate(`/chat/${chatItem.chatId}`);
        else
            navigate(`/chat`);
    };


    return <div className="chatcard">
        <Button variant='text' id='Button'
            onClick={handleClick}>
            <img src={thumbnail} width={'50px'}
            height={'50px'}></img>
            {username != null ? username : 'USERNAME NOT FOUND'}
            </Button>
    </div>
}