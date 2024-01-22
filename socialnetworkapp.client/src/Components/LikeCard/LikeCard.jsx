import { Link } from "react-router-dom"
import './LikeCard.css'
import { Button } from "@mui/material"
import { CURRENT_USER, notificationController, userController } from "../../Constants"

export default function LikeCard({ props })
{

    async function  handleClick(val)
    {
        const user = JSON.parse(localStorage.getItem(CURRENT_USER))
        if (val === 'accept')
        {
            const request = await fetch(userController + `/AddFriend/${user.userId}/${props.notification.url}`,
                {
                    method:'PUT'
            })
            if (!request.ok)
            {
                alert('Uh oh')
                return
                }
        }
        const request = await fetch(notificationController + `/DeleteRequest/${props.notification.notificationId}/${user.userId}`,
            {
            method:'DELETE'
            })
        location.reload()
    }
    const {thumbnail,username,userId} = props
    return <div className="like_card_container">
        <img src={thumbnail} className="like_thumbnail"></img>
        <Link className="like_username" to={'/Profile/' + props.userId}>{username}</Link>
        {props.notification&& <div style={{
            display: 'flex', flexDirection:'row-reverse',justifyContent:'space-evenly'
        }}><Button onClick={ev=>handleClick('accept')}>Accept</Button>
            <Button onClick={ev=>handleClick('reject')}>Reject</Button>
        </div>}
    </div>
}