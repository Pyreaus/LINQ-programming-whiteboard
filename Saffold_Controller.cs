               // [...]  controller class definition up here ^       

        // PUT: api/Offer/Edit/5
        [ActionName("Edit"),HttpPut("[controller]/[action]/{id:int}")]
        public async Task<IActionResult<int?>> Edit([FromRoute] int id, [FromBody] CreateUpdateOfferViewModel createUpdateOfferViewModel)
        {   
            if (await offerService.GetById(id) is null or not Offer offer) return NotFound(id); 
          
            Offer offerToUpdate = offerService.GetById(id);                                                            
            offerToUpdate.Caption = createUpdateOfferViewModel.Caption;           //
            offerToUpdate.Description = createUpdateOfferViewModel.Description;  //
            offerToUpdate.ImgPath = createUpdateOfferViewModel.ImgPath;         //
            offerService.Update(offerToUpdate);                                // I should be using AutoMapper here with Ignore() in map configuration 

            return NoContent();
        }
        // GET: api/Offer/GetOffer/5
        [ActionName("GetOffer"),HttpGet("[controller]/[action]/{id:int}")]
        public async Task<IActionResult<int?, OfferViewModel?>> GetOffer([FromRoute] int id)
        {
            if (await offerService.GetById(id) is null or not Offer offer) return NotFound(id); 
            Offer offer = offerService.GetById(id);

            PeopleFinderUser peopleFinderUser = userService.GetById(offer.CreatorId);
            OfferViewModel offerVM = new OfferViewModel
            {
                Id = offer.Id,
                ImgPath = offer.ImgPath,
                Caption = offer.Caption,
                Description = offer.Description,
                Timestamped = offer.Timestamped,
                Email = peopleFinderUser.Email,
                Telephone = peopleFinderUser.Telephone,
                FirstName = peopleFinderUser.FirstName,
                LastName = peopleFinderUser.LastName,
                Photo = bnetUrl + peopleFinderUser.Photo
            };
            
            return Ok(offerVM);
        }
         // DELETE: api/Offer/DeleteOffer/5
        [ActionName("DeleteOffer"),HttpDelete("[controller]/[action]/{id:int}")]
        public async Task<IActionResult<Offer?,int?>> DeleteOffer([FromRoute] int id)
        {
            if (await offerService.GetById(id) is null or not Offer offer) return NotFound(id); 
          
            Offer offerToDelete = offerService.GetById(id);
            offerService.Delete(offerToDelete);

            return Ok(offerToDelete);
        }



//TODO: create an Upload() controller for file uploads 
